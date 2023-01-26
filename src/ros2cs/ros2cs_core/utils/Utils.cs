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
using System.Runtime.InteropServices;

namespace ROS2
{
  /// <summary> Internal utilities for ros2cs_core </summary>
  internal static class Utils
  {
    /// <summary> Helper checker and converter of rcl return values to exceptions </summary>
    internal static void CheckReturnEnum(int ret)
    {
      string errorMessage = Utils.PopRclErrorString();

      switch ((RCLReturnEnum)ret)
      {
        case RCLReturnEnum.RCL_RET_OK:
          break;
        case RCLReturnEnum.RCL_RET_NODE_INVALID_NAME:
          throw new InvalidNodeNameException(errorMessage);
        case RCLReturnEnum.RCL_RET_NODE_INVALID_NAMESPACE:
          throw new InvalidNamespaceException(errorMessage);
        case RCLReturnEnum.RCL_RET_WAIT_SET_EMPTY:
          throw new WaitSetEmptyException(errorMessage);
        default:
          throw new RuntimeError(errorMessage);
      }
    }

    /// <summary> Get last rcl error </summary>
    /// <returns> String with error message </returns>
    internal static string GetRclErrorString()
    {
      IntPtr errorStringPtr = NativeRclInterface.rclcs_get_error_string();
      string errorString = PtrToString(errorStringPtr);
      NativeRclInterface.rclcs_dispose_error_string(errorStringPtr);
      return errorString;
    }

    /// <summary> Get and clean last rcl error </summary>
    /// <returns> String with error message </returns>
    internal static string PopRclErrorString()
    {
      string errorString = GetRclErrorString();
      NativeRcl.rcl_reset_error();
      return errorString;
    }

    /// <summary> Marshal a pointer to string </summary>
    /// <returns> String or null if the pointer was Zero </returns>
    internal static string PtrToString(IntPtr p)
    {
      if (p == IntPtr.Zero)
      {
        return null;
      }
      return Marshal.PtrToStringAnsi(p);
    }
  }
}
