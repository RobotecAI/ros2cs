// Copyright 2021 Robotec.ai
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
  namespace Internal
  { // TODO (adamdbrw) a namespace to warn where design did not deliver.

    /// <summary> Message interface that is meant to be internal </summary>
    /// <remarks> Not sure if it is possible to make this internal
    /// Note that this has to be visible between message assemblies (e.g. calling for nested messages)
    /// as well as for all messages (custom, generated, in principle unknown to ros2cs_core).
    /// It also needs to be visible to ros2cs_core classes. </remarks>
    public interface MessageInternals
    {
      IntPtr Handle { get; }
      IntPtr TypeSupportHandle { get; }
      void ReadNativeMessage();
      void WriteNativeMessage();
    }

    /// <summary> An utility class to acquire type support for a given message type </summary>
    internal static class MessageTypeSupportHelper
    {
      internal static IntPtr GetTypeSupportHandle<T>() where T : Message, new()
      {
        T msg = new T();
        IntPtr typeSupportHandle = (msg as MessageInternals).TypeSupportHandle;
        msg.Dispose();
        return typeSupportHandle;
      }
    }
  } // namespace Internal
} // namespace ROS2
