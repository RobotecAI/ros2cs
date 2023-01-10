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
    internal sealed class Node : INode
    {
        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public IContext Context { get { return this.ROSContext; } }

        /// <inheritdoc/>
        public IExecutor Executor { get; set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get { return !NativeRclInterface.rclcs_node_is_valid(this.Handle); }
        }

        internal IntPtr Handle = IntPtr.Zero;

        private IntPtr Options;

        private readonly Context ROSContext;

        /// <inheritdoc/>
        public IReadOnlyCollection<IPublisherBase> Publishers { get { return this.CurrentPublishers; } }

        private readonly HashSet<IRawPublisher> CurrentPublishers = new HashSet<IRawPublisher>();

        /// <inheritdoc/>
        public IReadOnlyCollection<ISubscriptionBase> Subscriptions { get { return this.CurrentSubscriptions; } }

        private readonly HashSet<IRawSubscription> CurrentSubscriptions = new HashSet<IRawSubscription>();

        /// <inheritdoc/>
        public IReadOnlyCollection<IServiceBase> Services { get { return this.CurrentServices; } }

        private readonly HashSet<IRawService> CurrentServices = new HashSet<IRawService>();

        /// <inheritdoc/>
        public IReadOnlyCollection<IClientBase> Clients { get { return this.CurrentClients; } }

        private readonly HashSet<IRawClient> CurrentClients = new HashSet<IRawClient>();

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
        public IPublisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            this.AssertOk();
            Publisher<T> publisher = new Publisher<T>(topic, this, qos);
            bool success = this.CurrentPublishers.Add(publisher);
            Debug.Assert(success, "publisher already exists");
            return publisher;
        }

        /// <summary>
        /// Remove a publisher.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Publisher.Dispose"/> and does not dispose the publisher.
        /// </remarks>
        /// <param name="publisher">Publisher to be removed.</param>
        /// <returns>If the publisher existed on this node and has been removed.</returns>
        internal bool RemovePublisher(IRawPublisher publisher)
        {
            return this.CurrentPublishers.Remove(publisher);
        }

        /// <inheritdoc/>
        public ISubscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            this.AssertOk();
            Subscription<T> subscription = new Subscription<T>(topic, this, callback, qos);
            bool success = this.CurrentSubscriptions.Add(subscription);
            Debug.Assert(success, "subscription already exists");
            this.Executor?.TryScheduleRescan(this);
            return subscription;
        }

        /// <summary>
        /// Remove a subscription.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Subscription.Dispose"/> and does not dispose the subscription.
        /// </remarks>
        /// <param name="subscription">Subscription to be removed.</param>
        /// <returns>If the subscription existed on this node and has been removed.</returns>
        internal bool RemoveSubscription(IRawSubscription subscription)
        {
            if (this.CurrentSubscriptions.Remove(subscription))
            {
                this.Executor?.TryScheduleRescan(this);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public IClient<I, O> CreateClient<I, O>(string topic, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            this.AssertOk();
            Client<I, O> client = new Client<I, O>(topic, this, qos);
            bool success = this.CurrentClients.Add(client);
            Debug.Assert(success, "client already exists");
            this.Executor?.TryScheduleRescan(this);
            return client;
        }

        /// <summary>
        /// Remove a client.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Client.Dispose"/> and does not dispose the client.
        /// </remarks>
        /// <param name="client">Client to be removed.</param>
        /// <returns>If the client existed on this node and has been removed.</returns>
        internal bool RemoveClient(IRawClient client)
        {
            if (this.CurrentClients.Remove(client))
            {
                this.Executor?.TryScheduleRescan(this);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public IService<I, O> CreateService<I, O>(string topic, Func<I, O> callback, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            this.AssertOk();
            Service<I, O> service = new Service<I, O>(topic, this, callback, qos);
            bool success = this.CurrentServices.Add(service);
            Debug.Assert(success, "service already exists");
            this.Executor?.TryScheduleRescan(this);
            return service;
        }

        /// <summary>
        /// Remove a service.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Service.Dispose"/> and does not dispose the service.
        /// </remarks>
        /// <param name="service">Service to be removed.</param>
        /// <returns>If the service existed on this node and has been removed.</returns>
        internal bool RemoveService(IRawService service)
        {
            if (this.CurrentServices.Remove(service))
            {
                this.Executor?.TryScheduleRescan(this);
                return true;
            }
            return false;
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

            if (!(this.Executor is null))
            {
                bool success = this.Executor.Remove(this);
                Debug.Assert(success, "node was not added to its old executor");
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
