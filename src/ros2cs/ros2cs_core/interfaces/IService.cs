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

namespace ROS2
{
  /// <summary> Non-generic base interface for all subscriptions </summary>
  /// <seealso cref="INode.CreateService"/>
  public interface IServiceBase : IExtendedDisposable
  {
    /// <summary>
    /// Tries to get a request message from rcl/rmw layers
    /// </summary>
    /// <remarks>Invokes the callback if successful</remarks>
    // TODO(adamdbrw) this should not be public - add an internal interface
    void TakeMessage();

    /// <summary>
    /// topic name which was used when calling <see cref="INode.CreateService"/>
    /// </summary>
    string Topic {get;}

    // TODO(adamdbrw) this should not be public - add an internal interface
    rcl_service_t Handle {get;}

    /// <summary> service mutex for internal use </summary>
    object Mutex { get; }
  }

  /// <summary> Generic base interface for all services </summary>
  /// <typeparam name="I">Message Type to be received</typeparam>
  /// <typeparam name="O">Message Type to be send</typeparam>
  /// <seealso cref="INode.CreateService"/>
  public interface IService<I, O>: IServiceBase
    where I: Message
    where O: Message
  {
  }
}
