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
  /// <description> Use Ros2cs.CreateClient to construct.
  /// This interface is useful for managing service collections and disposal </description>
  public interface IClientBase: IExtendedDisposable
  {
    string Topic {get;}
  }

  /// <summary> Generic base interface for all subscriptions </summary>
  public interface IClient<T>: IClientBase
      where T: Message
  {
    void WaitForService(T msg);
    /// <summary> Service a message </summary>
    /// <description> Message memory is copied into native structures and the message
    /// can be safely changed or disposed after this call </description>
    IntPtr SendAndRecv(T msg);
  }
}
