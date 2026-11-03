// Copyright 2019-2023 Robotec.ai
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
    /// <summary> Non-generic base interface for all services. </summary>
    /// <remarks>
    /// This interface is useful for managing service collections and disposal.
    /// Create instances with <see cref="INode.CreateService"/>.
    /// </remarks>
    public interface IServiceBase : IExtendedDisposable, IWaitable
    {
        /// <summary> Topic of this service. </summary>
        string Topic { get; }
    }

    /// <summary> Internal service extensions. </summary>
    internal interface IRawService : IServiceBase
    {
        /// <summary> Dispose without modifying the node. </summary>
        void DisposeFromNode();
    }

    /// <summary> Generic base interface for all services </summary>
    /// <typeparam name="I"> Message Type to be received. </typeparam>
    /// <typeparam name="O"> Message Type to be send. </typeparam>
    public interface IService<I, O> : IServiceBase
      where I : Message
      where O : Message
    {
    }
}
