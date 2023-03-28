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
    /// <summary>
    /// Node wrapping a rcl node.
    /// </summary>
    /// <remarks>
    /// This is the implementation produced by <see cref="Context.TryCreateNode"/>,
    /// use this method to create instances.
    /// </remarks>
    /// <seealso cref="ROS2.Context"/>
    /// <inheritdoc cref="INode"/>
    public sealed class Node : INode
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

        /// <summary>
        /// Handle to the rcl node
        /// </summary>
        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Handle to the rcl node options
        /// </summary>
        private IntPtr Options = IntPtr.Zero;

        /// <summary>
        /// Context associated with this instance.
        /// </summary>
        private readonly Context ROSContext;

        /// <summary>
        /// Lock used to allow thread safe access to node primitives.
        /// </summary>
        private readonly object Lock = new object();

        /// <remarks>
        /// This collection is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IReadOnlyCollection<IPublisherBase> Publishers { get; private set; }

        private readonly HashSet<IRawPublisher> CurrentPublishers = new HashSet<IRawPublisher>();

        /// <remarks>
        /// This collection is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IReadOnlyCollection<ISubscriptionBase> Subscriptions { get; private set; }

        private readonly HashSet<IRawSubscription> CurrentSubscriptions = new HashSet<IRawSubscription>();

        /// <remarks>
        /// This collection is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IReadOnlyCollection<IServiceBase> Services { get; private set; }

        private readonly HashSet<IRawService> CurrentServices = new HashSet<IRawService>();

        /// <remarks>
        /// This collection is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IReadOnlyCollection<IClientBase> Clients { get; private set; }

        private readonly HashSet<IRawClient> CurrentClients = new HashSet<IRawClient>();

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for adding the instance to <paramref name="context"/>.
        /// This action is not thread safe.
        /// </remarks>
        /// <param name="name"> Name of the node. </param>
        /// <param name="context"> Context to associate with. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="context"/> is disposed. </exception>
        internal Node(string name, Context context)
        {
            this.Name = name;
            this.ROSContext = context;
            this.Publishers = new LockedCollection<IPublisherBase>(this.CurrentPublishers, this.Lock);
            this.Subscriptions = new LockedCollection<ISubscriptionBase>(this.CurrentSubscriptions, this.Lock);
            this.Services = new LockedCollection<IServiceBase>(this.CurrentServices, this.Lock);
            this.Clients = new LockedCollection<IClientBase>(this.CurrentClients, this.Lock);

            this.Options = NativeRclInterface.rclcs_node_create_default_options();
            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_node();
            int ret = NativeRcl.rcl_node_init(
              this.Handle,
              this.Name,
              "/",
              this.ROSContext.Handle,
              this.Options

            );
            switch ((RCLReturnEnum)ret)
            {
                case RCLReturnEnum.RCL_RET_OK:
                    break;
                // does not return RCL_RET_NOT_INIT if the context is NULL
                case RCLReturnEnum.RCL_RET_INVALID_ARGUMENT:
                    this.FreeHandles();
                    throw new ObjectDisposedException("RCL Context");
                default:
                    this.FreeHandles();
                    Utils.CheckReturnEnum(ret);
                    break;
            }
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <seealso cref="Publisher{T}"/>
        /// <inheritdoc/>
        public IPublisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            lock (this.Lock)
            {
                Publisher<T> publisher = new Publisher<T>(topic, this, qos);
                bool success = this.CurrentPublishers.Add(publisher);
                Debug.Assert(success, "publisher already exists");
                return publisher;
            }
        }

        /// <summary>
        /// Remove a publisher.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Publisher.Dispose"/> and does not dispose the publisher.
        /// Furthermore, it is thread safe.
        /// </remarks>
        /// <param name="publisher">Publisher to be removed.</param>
        /// <returns>If the publisher existed on this node and has been removed.</returns>
        internal bool RemovePublisher(IRawPublisher publisher)
        {
            lock (this.Lock)
            {
                return this.CurrentPublishers.Remove(publisher);
            }
        }

        /// <remarks>
        /// This method schedules a rescan on the current executor and is thread safe
        /// if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <seealso cref="Subscription{T}"/>
        /// <inheritdoc/>
        public ISubscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            Subscription<T> subscription;
            lock (this.Lock)
            {
                subscription = new Subscription<T>(topic, this, callback, qos);
                bool success = this.CurrentSubscriptions.Add(subscription);
                Debug.Assert(success, "subscription already exists");
            }
            this.Executor?.TryScheduleRescan(this);
            return subscription;
        }

        /// <summary>
        /// Remove a subscription.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Subscription.Dispose"/> and does not dispose the subscription.
        /// Furthermore, it is thread safe if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <param name="subscription">Subscription to be removed.</param>
        /// <returns>If the subscription existed on this node and has been removed.</returns>
        internal bool RemoveSubscription(IRawSubscription subscription)
        {
            bool removed;
            lock (this.Lock)
            {
                removed = this.CurrentSubscriptions.Remove(subscription);
            }
            if (removed)
            {
                this.Executor?.TryScheduleRescan(this);
            }
            return removed;
        }

        /// <remarks>
        /// This method schedules a rescan on the current executor and is thread safe
        /// if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <seealso cref="Client{I, O}"/>
        /// <inheritdoc/>
        public IClient<I, O> CreateClient<I, O>(string topic, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            Client<I, O> client;
            lock (this.Lock)
            {
                client = new Client<I, O>(topic, this, qos);
                bool success = this.CurrentClients.Add(client);
                Debug.Assert(success, "client already exists");
            }
            this.Executor?.TryScheduleRescan(this);
            return client;
        }

        /// <summary>
        /// Remove a client.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Client.Dispose"/> and does not dispose the client.
        /// Furthermore, it is thread safe if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <param name="client">Client to be removed.</param>
        /// <returns>If the client existed on this node and has been removed.</returns>
        internal bool RemoveClient(IRawClient client)
        {
            bool removed;
            lock (this.Lock)
            {
                removed = this.CurrentClients.Remove(client);
            }
            if (removed)
            {
                this.Executor?.TryScheduleRescan(this);
            }
            return removed;
        }

        /// <remarks>
        /// This method schedules a rescan on the current executor and is thread safe
        /// if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <seealso cref="Service{I, O}"/>
        /// <inheritdoc/>
        public IService<I, O> CreateService<I, O>(string topic, Func<I, O> callback, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new()
        {
            Service<I, O> service;
            lock (this.Lock)
            {
                service = new Service<I, O>(topic, this, callback, qos);
                bool success = this.CurrentServices.Add(service);
                Debug.Assert(success, "service already exists");
            }
            this.Executor?.TryScheduleRescan(this);
            return service;
        }

        /// <summary>
        /// Remove a service.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Service.Dispose"/> and does not dispose the service.
        /// Furthermore, it is thread safe if <see cref="IExecutor.TryScheduleRescan"/> of the current executor is thread safe.
        /// </remarks>
        /// <param name="service">Service to be removed.</param>
        /// <returns>If the service existed on this node and has been removed.</returns>
        internal bool RemoveService(IRawService service)
        {
            bool removed;
            lock (this.Lock)
            {
                removed = this.CurrentServices.Remove(service);
            }
            if (removed)
            {
                this.Executor?.TryScheduleRescan(this);
            }
            return removed;
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the node or any of its primitives are in use.
        /// Furthermore, it is NOT performed on finalization by the GC.
        /// </remarks>
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

        /// <summary>
        /// Free the rcl handles and replace them with null pointers.
        /// </summary>
        /// <remarks>
        /// The handles are not finalised by this method.
        /// </remarks>
        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_node(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_node_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }
    }
}
