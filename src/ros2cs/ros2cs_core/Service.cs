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
using System.Diagnostics;
using System.Threading.Tasks;
using ROS2.Internal;

namespace ROS2
{
    /// <summary> Service with a topic and Types for Messages. </summary>
    /// <inheritdoc cref="IService{I, O}"/>
    internal sealed class Service<I, O> : IService<I, O>, IRawService
    where I : Message, new()
    where O : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRclInterface.rclcs_service_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        public IntPtr Handle { get; private set; } = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private readonly Node Node;

        /// <summary>
        /// Callback to be called to process incoming requests
        /// </summary>
        private readonly Func<I, O> Callback;

        internal Service(string topic, Node node, Func<I, O> callback, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;
            this.Callback = callback;

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);

            this.Options = NativeRclInterface.rclcs_service_create_options(qualityOfServiceProfile.handle);

            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<I>();

            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_service();
            int ret = NativeRcl.rcl_service_init(
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

        /// <inheritdoc/>
        public bool TryProcess()
        {
            if (this.IsDisposed)
            {
                return false;
            }

            rcl_rmw_request_id_t header = default(rcl_rmw_request_id_t);
            I message = new I();
            int ret = NativeRcl.rcl_take_request(
                this.Handle,
                ref header,
                (message as MessageInternals).Handle
            );
            GC.KeepAlive(this);

            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_SERIVCE_TAKE_FAILD)
            {
                Utils.CheckReturnEnum(ret);
                this.ProcessRequest(header, message);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public Task<bool> TryProcessAsync()
        {
            return Task.FromResult(this.TryProcess());
        }

        /// <summary>
        /// Populates managed fields with native values and calls the callback with the created message
        /// </summary>
        /// <remarks>Sending the Response is also takes care of by this method</remarks>
        /// <param name="message">Message that will be populated and provided to the callback</param>
        /// <param name="header">request id received when taking the Request</param>
        private void ProcessRequest(rcl_rmw_request_id_t header, I message)
        {
            (message as MessageInternals).ReadNativeMessage();
            this.SendResp(header, this.Callback(message));
        }

        /// <summary>
        /// Send Response Message with rcl/rmw layers
        /// </summary>
        /// <param name="header">request id received when taking the Request</param>
        /// <param name="msg">Message to be send</param>
        private void SendResp(rcl_rmw_request_id_t header, O msg)
        {
            MessageInternals msgInternals = msg as MessageInternals;
            msgInternals.WriteNativeMessage();
            Utils.CheckReturnEnum(NativeRcl.rcl_send_response(
                this.Handle,
                ref header,
                msgInternals.Handle
            ));
            GC.KeepAlive(this);
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

            // only do if Node.CurrentServices has not been finalized
            if (disposing)
            {
                bool success = this.Node.RemoveService(this);
                Debug.Assert(success, "failed to remove service");
                // not required if we are being finalized since the executor cant be running
                this.Node.Executor?.Wait();
            }

            this.DisposeFromNode();
        }

        /// <inheritdoc/>
        public void DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_service_fini(this.Handle, this.Node.Handle));
            this.FreeHandles();
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_service(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_service_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }

        ~Service()
        {
            this.Dispose(false);
        }
    }
}
