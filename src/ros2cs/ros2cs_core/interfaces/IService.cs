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
  /// <description> Use Ros2cs.CreateSubscription to construct </description>
  public interface IServiceBase : IExtendedDisposable
  {
    // TODO(adamdbrw) this should not be public - add an internal interface
    void SendResp(IntPtr valp);

    // TODO(adamdbrw) this should not be public - add an internal interface
    void TakeMessage();

    /// <summary> topic name which was used when calling Ros2cs.CreateService </summary>
    string Topic {get;}

    // TODO(adamdbrw) this should not be public - add an internal interface
    rcl_service_t Handle {get;}

    /// <summary> service mutex for internal use </summary>
    object Mutex { get; }
  }

  /// <summary> Generic base interface for all services </summary>
  public interface IService<T>: IServiceBase where T: Message {}
}
