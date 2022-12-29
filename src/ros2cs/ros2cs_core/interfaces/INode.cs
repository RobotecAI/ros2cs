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

namespace ROS2
{
    /// <summary>
    /// Ros2cs node, created with <see cref="IContext.TryCreateNode(string, out INode)"/>.
    /// </summary>
    /// <remarks>
    /// Instances should be disposed with <see cref="Dispose"/> which is NOT automatically performed on shutdown. 
    /// </remarks>
    public interface INode : IExtendedDisposable
    {
        /// <summary>
        /// Node name as given in <see cref="IContext.TryCreateNode(string, out INode)"/>.
        /// </summary>
        /// <remarks>Is unique per context while node is not disposed.</remarks>
        string Name { get; }

        /// <summary>
        /// Context containing this node.
        /// </summary>
        IContext Context { get; }

        /// <summary>
        /// Executor handling callbacks of this node.
        /// </summary>
        /// <remarks>
        /// A node has to guarantee that it is associated with at most one executor at any given time
        /// to prevent undefined behaviour when multithreading is used.
        /// </remarks>
        IExecutor Executor { get; }

        /// <summary>
        /// Try to change the executor of this node.
        /// </summary>
        /// <remarks>
        /// A node has to guarantee that it is associated with at most one executor at any given time
        /// to prevent undefined behaviour when multithreading is used.
        /// </remarks>
        /// <param name="executor">The new executor</param>
        /// <returns>If the change was successful</returns>
        bool TrySetExecutor(IExecutor executor);

        /// <inheritdoc cref="TrySetExecutor"/>
        /// <param name="oldExecutor">The old executor</param>
        bool TrySetExecutor(IExecutor executor, out IExecutor oldExecutor);

        /// <summary> Create a publisher for this node for a given topic, qos and message type </summary>
        /// <param name="topic"> Topic for the publisher. Naming restrictions of ros2 apply and violation results in an exception </param>
        /// <param name="qos"> Quality of Service settings. Not passing this parameter will result in default settings </param>
        /// <returns> Publisher for the topic, which can be used to publish messages </returns>
        IPublisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new();

        /// <summary>
        /// Publishers created on this node.
        /// </summary>
        IReadOnlyCollection<IPublisherBase> Publishers { get; }

        /// <summary> Create a subscription for this node for a given topic, callback, qos and message type </summary>
        /// <param name="topic"> Topic to subscribe to. Naming restrictions of ros2 apply and violation results in an exception </param>
        /// <param name="callback"> Action to be called when message is received (through Spin or SpinOnce). Provide a lambda or a method </param>
        /// <param name="qos"> Quality of Service settings. Not passing this parameter will result in default settings </param>
        /// <returns> Subscription for the topic </returns>
        ISubscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new();

        /// <summary>
        /// Subscriptions created on this node.
        /// </summary>
        IReadOnlyCollection<ISubscriptionBase> Subscriptions { get; }

        /// <summary> Create a client for this node for a given topic, qos and message type </summary>
        /// <param name="topic"> Topic for the client. Naming restrictions of ros2 apply and violation results in an exception </param>
        /// <param name="qos"> Quality of Client settings. Not passing this parameter will result in default settings </param>
        /// <returns> Client for the topic, which can be used to client messages </returns>
        IClient<I, O> CreateClient<I, O>(string topic, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new();

        /// <summary>
        /// Clients created on this node.
        /// </summary>
        IReadOnlyCollection<IClientBase> Clients { get; }

        /// <summary> Create a service for this node for a given topic, callback, qos and message type </summary>
        /// <param name="topic"> Topic to service to. Naming restrictions of ros2 apply and violation results in an exception </param>
        /// <param name="callback"> Action to be called when message is received. Provide a lambda or a method </param>
        /// <param name="qos"> Quality of Service settings. Not passing this parameter will result in default settings </param>
        /// <returns> Service for the topic </returns>
        IService<I, O> CreateService<I, O>(string topic, Func<I, O> callback, QualityOfServiceProfile qos = null) where I : Message, new() where O : Message, new();

        /// <summary>
        /// Services created on this node.
        /// </summary>
        IReadOnlyCollection<IServiceBase> Services { get; }
    }
}
