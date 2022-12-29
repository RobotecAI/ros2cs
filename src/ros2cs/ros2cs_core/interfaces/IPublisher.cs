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

namespace ROS2
{
    /// <summary> Non-generic base interface for all publishers. </summary>
    /// <remarks>
    /// This interface is useful for managing publisher collections and disposal.
    /// Create instances with <see cref="INode.CreatePublisher"/>.
    /// </remarks>
    public interface IPublisherBase : IExtendedDisposable
    {
        /// <summary> Topic of this publisher. </summary>
        string Topic { get; }
    }

    /// <summary> Internal publisher extensions. </summary>
    internal interface IRawPublisher : IPublisherBase
    {
        /// <summary> Dispose without modifying the node. </summary>
        void DisposeFromNode();
    }

    /// <summary> Generic base interface for all publishers. </summary>
    /// <typeparam name="T"> Message Type to be published. </typeparam>
    public interface IPublisher<T> : IPublisherBase
        where T : Message
    {
        /// <summary> Publish a message </summary>
        /// <remarks>
        /// Message memory is copied into native structures and the message
        /// can be safely changed or disposed after this call.
        /// </remarks>
        /// <param name="msg"> Message to be published. </param>
        void Publish(T msg);
    }
}
