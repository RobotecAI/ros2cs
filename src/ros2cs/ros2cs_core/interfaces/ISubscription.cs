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
    /// <summary> Non-generic base interface for all subscriptions. </summary>
    /// <remarks>
    /// This interface is useful for managing subscription collections and disposal.
    /// Create instances with <see cref="INode.CreateSubscription"/>.
    /// </remarks>
    public interface ISubscriptionBase : IExtendedDisposable, IWaitable
    {
        /// <summary> Topic of this subscription. </summary>
        string Topic { get; }
    }

    /// <summary> Internal subscription extensions. </summary>
    internal interface IRawSubscription : ISubscriptionBase
    {
        /// <summary> Dispose without modifying the node. </summary>
        void DisposeFromNode();
    }

    /// <summary> Generic base interface for all subscriptions. </summary>
    /// <typeparam name="T"> Message Type to be received. </typeparam>
    public interface ISubscription<T> : ISubscriptionBase where T : Message { }
}
