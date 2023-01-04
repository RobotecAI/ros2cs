// Copyright 2019-2021 Robotec.ai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ROS2.Internal;


namespace ROS2
{
    /// <summary> Client with a topic and Types for Messages. </summary>
    /// <inheritdoc cref="IClient{I, O}"/>
    internal sealed class Client<I, O> : IClient<I, O>, IRawClient
    where I : Message, new()
    where O : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<long, Task<O>> PendingRequests { get; private set; }

        /// <inheritdoc/>
        IReadOnlyDictionary<long, Task> IClientBase.PendingRequests { get { return this.UntypedPendingRequests; } }

        private readonly IReadOnlyDictionary<long, Task> UntypedPendingRequests;

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRcl.rcl_client_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        private IntPtr Handle = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private readonly Node Node;

        /// <summary>
        /// Mapping from request id without Response to <see cref="TaskCompletionSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="TaskCompletionSource.Task"/> is stored separately to allow
        /// <see cref="Cancel"/> to work even if the source returns multiple tasks.
        /// </remarks>
        private readonly Dictionary<long, (TaskCompletionSource<O>, Task<O>)> Requests = new Dictionary<long, (TaskCompletionSource<O>, Task<O>)>();

        /// <summary>
        /// Internal constructor for Client
        /// </summary>
        /// <remarks>Use <see cref="INode.CreateClient"/> to construct new Instances</remarks>
        public Client(string topic, Node node, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;

            var lockedRequests = new LockedDictionary<long, (TaskCompletionSource<O>, Task<O>)>(this.Requests);
            this.PendingRequests = new MappedValueDictionary<long, (TaskCompletionSource<O>, Task<O>), Task<O>>(
                lockedRequests,
                tuple => tuple.Item2
            );
            this.UntypedPendingRequests = new MappedValueDictionary<long, (TaskCompletionSource<O>, Task<O>), Task>(
                lockedRequests,
                tuple => tuple.Item2
            );

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);

            this.Options = NativeRclInterface.rclcs_client_create_options(qualityOfServiceProfile.handle);

            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<I>();

            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_client();
            int ret = NativeRcl.rcl_client_init(
                this.Handle,
                this.Node.Handle,
                typeSupportHandle,
                this.Topic,
                this.Options
            );

            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
        }

        /// <summary>
        /// Assert that the client has not been disposed.
        /// </summary>
        private void AssertOk()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException($"client for topic '{this.Topic}'");
            }
        }

        /// <inheritdoc/>
        public bool IsServiceAvailable()
        {
            this.AssertOk();
            bool available = false;
            Utils.CheckReturnEnum(NativeRcl.rcl_service_server_is_available(
                this.Node.Handle,
                this.Handle,
                ref available
            ));
            GC.KeepAlive(this);
            return available;
        }

        /// <inheritdoc/>
        public bool TryProcess()
        {
            if (this.IsDisposed)
            {
                return false;
            }

            rcl_rmw_request_id_t header = default(rcl_rmw_request_id_t);
            O message = new O();
            (TaskCompletionSource<O>, Task<O>) source;
            bool exists = false;

            lock (this.Requests)
            {
                // prevent taking responses before RegisterSource was called
                int ret = NativeRcl.rcl_take_response(
                    this.Handle,
                    ref header,
                    (message as MessageInternals).Handle
                );
                GC.KeepAlive(this);

                if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_CLIENT_TAKE_FAILED)
                {
                    return false;
                }
                Utils.CheckReturnEnum(ret);
                if (this.Requests.TryGetValue(header.sequence_number, out source))
                {
                    exists = true;
                    this.Requests.Remove(header.sequence_number);
                }
            }
            if (exists)
            {
                (message as MessageInternals).ReadNativeMessage();
                source.Item1.SetResult(message);
            }
            else
            {
                Debug.Print("received request which was not pending, maybe canceled");
            }
            return true;
        }

        /// <inheritdoc/>
        public Task<bool> TryProcessAsync()
        {
            return Task.FromResult(this.TryProcess());
        }

        /// <inheritdoc/>
        public O Call(I msg)
        {
            var task = CallAsync(msg);
            task.Wait();
            return task.Result;
        }

        /// <inheritdoc/>
        public Task<O> CallAsync(I msg)
        {
            return CallAsync(msg, TaskCreationOptions.None);
        }

        /// <inheritdoc/>
        public Task<O> CallAsync(I msg, TaskCreationOptions options)
        {
            this.AssertOk();
            var source = new TaskCompletionSource<O>(options);
            lock (this.Requests)
            {
                // prevents TryProcess from receiving Responses before we called RegisterSource
                long sequence_number = SendRequest(msg);
                return RegisterSource(source, sequence_number);
            }
        }

        /// <summary>
        /// Send a Request to the Service
        /// </summary>
        /// <param name="msg">Message to be send</param>
        /// <returns>sequence number of the Request</returns>
        private long SendRequest(I msg)
        {
            long sequence_number = default(long);
            MessageInternals msgInternals = msg as MessageInternals;
            msgInternals.WriteNativeMessage();
            Utils.CheckReturnEnum(
                NativeRcl.rcl_send_request(
                    this.Handle,
                    msgInternals.Handle,
                    ref sequence_number
                )
            );
            GC.KeepAlive(this);
            return sequence_number;
        }

        /// <summary>
        /// Associate a task with a sequence number
        /// </summary>
        /// <param name="source">source used to controll the <see cref="Task"/></param>
        /// <param name="sequence_number">sequence number received when sending the Request</param>
        /// <returns>The associated task.</returns>
        private Task<O> RegisterSource(TaskCompletionSource<O> source, long sequence_number)
        {
            // handle Task not being a singleton
            Task<O> task = source.Task;
            Requests.Add(sequence_number, (source, task));
            return task;
        }

        /// <inheritdoc/>
        public bool Cancel(Task task)
        {
            var pair = default(KeyValuePair<long, (TaskCompletionSource<O>, Task<O>)>);
            lock (this.Requests)
            {
                try
                {
                    pair = this.Requests.First(entry => entry.Value.Item2 == task);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                // has to be true
                bool success = this.Requests.Remove(pair.Key);
                Debug.Assert(success, "failed to remove matching request");
            }
            pair.Value.Item1.SetCanceled();
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            // finalizer not needed when we disposed successfully
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposal logic.</summary>
        /// <param name="disposing">If this method is not called in a finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            // only do if Node.CurrentClients and this.Requests have not been finalized
            if (disposing)
            {
                bool success = this.Node.RemoveClient(this);
                Debug.Assert(success, "failed to remove client");
                this.Node.Executor?.Wake(this.Node);
                this.DisposeAllTasks();
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_client_fini(this.Handle, this.Node.Handle));
            this.FreeHandles();
        }

        /// <inheritdoc/>
        public void DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            this.DisposeAllTasks();
            Utils.CheckReturnEnum(NativeRcl.rcl_client_fini(this.Handle, this.Node.Handle));
            this.FreeHandles();
        }

        private void DisposeAllTasks()
        {
            lock (this.Requests)
            {
                foreach (var source in this.Requests.Values)
                {
                    source.Item1.TrySetException(new ObjectDisposedException($"client for topic '{this.Topic}'"));
                }
                this.Requests.Clear();
            }
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_client(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_client_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }

        ~Client()
        {
            this.Dispose(false);
        }
    }
}
