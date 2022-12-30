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

namespace ROS2
{
    /// <inheritdoc cref="INode"/>
    internal sealed class Node : INode, IExtendedDisposable
    {
        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public IContext Context { get { return this.ROSContext; } }

        /// <inheritdoc/>
        public IExecutor Executor { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get { return !NativeRcl.rcl_node_is_valid(this.Handle); }
        }

        internal IntPtr Handle = IntPtr.Zero;

        private Context ROSContext;

        private IntPtr Options;

        /// <inheritdoc/>
        public IReadOnlyCollection<IPublisherBase> Publishers { get { return this.CurrentPublishers; } }

        internal HashSet<IRawPublisher> CurrentPublishers = new HashSet<IRawPublisher>();

        /// <inheritdoc/>
        public IReadOnlyCollection<ISubscriptionBase> Subscriptions { get { return this.CurrentSubscriptions; } }

        internal HashSet<IRawSubscription> CurrentSubscriptions = new HashSet<IRawSubscription>();

        /// <inheritdoc/>
        public IReadOnlyCollection<IServiceBase> Services { get { return this.CurrentServices; } }

        internal HashSet<IRawService> CurrentServices = new HashSet<IRawService>();

        /// <inheritdoc/>
        public IReadOnlyCollection<IClientBase> Clients { get { return this.CurrentClients; } }

        internal HashSet<IRawClient> CurrentClients = new HashSet<IRawClient>();

        internal Node(string name, Context context)
        {
            this.Name = name;
            this.ROSContext = context;
            this.Options = NativeRclInterface.rclcs_node_create_default_options();
            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_node();
            int ret = NativeRcl.rcl_node_init(
              this.Handle,
              this.Name,
              "/",
              this.ROSContext.Handle,
              this.Options

            );
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
        }

        /// <summary>
        /// Assert that the instance has not been disposed.
        /// </summary>
        private void AssertOk()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException($"ROS2 node '{this.Name}'");
            }
        }

        /// <inheritdoc/>
        public bool TrySetExecutor(IExecutor executor)
        {
            return this.TrySetExecutor(executor, out _);
        }

        /// <inheritdoc/>
        public bool TrySetExecutor(IExecutor executor, out IExecutor oldExecutor)
        {
            oldExecutor = default(IExecutor);
            if (this.Executor != null && !this.Executor.Remove(this))
            {
                return false;
            }
            // prevent invalid executor if a failure occurs
            (oldExecutor, this.Executor) = (this.Executor, oldExecutor);
            oldExecutor?.Wake(this);
            executor?.Add(this);
            this.Executor = executor;
            executor?.Wake(this);
            return true;
        }

        /// <inheritdoc/>
        public IPublisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            this.AssertOk();
            Publisher<T> publisher = new Publisher<T>(topic, this, qos);
            bool success = this.CurrentPublishers.Add(publisher);
            Debug.Assert(success, "publisher already exists");
            return publisher;
        }

        /// <inheritdoc/>
        public ISubscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            this.AssertOk();
            Subscription<T> subscription = new Subscription<T>(topic, this, callback, qos);
            bool success = this.CurrentSubscriptions.Add(subscription);
            Debug.Assert(success, "subscription already exists");
            this.Executor?.Wake(this);
            return subscription;
        }

        /// <inheritdoc/>
        public IClient<I, O> CreateClient<I, O>(string topic, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            this.AssertOk();
            Client<I, O> client = new Client<I, O>(topic, this, qos);
            bool success = this.CurrentClients.Add(client);
            Debug.Assert(success, "client already exists");
            this.Executor?.Wake(this);
            return client;
        }

        /// <inheritdoc/>
        public IService<I, O> CreateService<I, O>(string topic, Func<I, O> callback, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            this.AssertOk();
            Service<I, O> service = new Service<I, O>(topic, this, callback, qos);
            bool success = this.CurrentServices.Add(service);
            Debug.Assert(success, "service already exists");
            this.Executor?.Wake(this);
            return service;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            bool success = this.ROSContext.RemoveNode(this.Name);
            Debug.Assert(success, "failed to remove node");

            // no finalizer since the hash sets may have been finalized
            this.DisposeFromContext();
        }

        /// <summary>
        /// Dispose this node without modifying the context.
        /// </summary>
        internal void DisposeFromContext()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            if (!this.TrySetExecutor(null))
            {
                throw new RuntimeError("removing the node from the current executor failed");
            }

            foreach (IRawPublisher publisher in this.CurrentPublishers)
            {
                publisher.DisposeFromNode();
            }
            this.CurrentPublishers.Clear();

            foreach (IRawSubscription subscription in this.CurrentSubscriptions)
            {
                subscription.DisposeFromNode();
            }
            this.CurrentSubscriptions.Clear();

            foreach (IRawService service in this.CurrentServices)
            {
                service.DisposeFromNode();
            }
            this.CurrentServices.Clear();

            foreach (IRawClient client in this.CurrentClients)
            {
                client.DisposeFromNode();
            }
            this.CurrentClients.Clear();

            Utils.CheckReturnEnum(NativeRcl.rcl_node_fini(this.Handle));
            this.FreeHandles();
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_node(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_node_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }
    }
}
