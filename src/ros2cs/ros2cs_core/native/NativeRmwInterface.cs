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
  /// <summary>
  /// An internal class to access wrappers to rmw calls, mostly for exposing Quality of Service
  /// </summary>
  internal static class NativeRmwInterface
  {
    private static readonly DllLoadUtils dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
    private static readonly IntPtr nativeRMW = dllLoadUtils.LoadLibrary("ros2cs");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr RMWImplementationIdentifier();
    internal static RMWImplementationIdentifier
        rmw_native_interface_get_implementation_identifier =
        (RMWImplementationIdentifier)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRMW,
        "rmw_native_interface_get_implementation_identifier"),
        typeof(RMWImplementationIdentifier));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr RMWNativeCreateQoSProfileIdentifierType(int preset_profile);
    internal static RMWNativeCreateQoSProfileIdentifierType
      rmw_native_interface_create_qos_profile =
      (RMWNativeCreateQoSProfileIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
      nativeRMW,
      "rmw_native_interface_create_qos_profile"),
      typeof(RMWNativeCreateQoSProfileIdentifierType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RMWNativeDeleteQoSProfileIdentifierType(IntPtr profile);
    internal static RMWNativeDeleteQoSProfileIdentifierType
      rmw_native_interface_delete_qos_profile =
      (RMWNativeDeleteQoSProfileIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
      nativeRMW,
      "rmw_native_interface_delete_qos_profile"),
      typeof(RMWNativeDeleteQoSProfileIdentifierType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RMWNativeSetHistoryIdentifierType(IntPtr profile, int mode, int depth);
    internal static RMWNativeSetHistoryIdentifierType
      rmw_native_interface_set_history =
      (RMWNativeSetHistoryIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
      nativeRMW,
      "rmw_native_interface_set_history"),
      typeof(RMWNativeSetHistoryIdentifierType));

      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      internal delegate void RMWNativeSetReliabilityIdentifierType(IntPtr profile, int mode);
      internal static RMWNativeSetReliabilityIdentifierType
        rmw_native_interface_set_reliability =
        (RMWNativeSetReliabilityIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRMW,
        "rmw_native_interface_set_reliability"),
        typeof(RMWNativeSetReliabilityIdentifierType));

      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      internal delegate void RMWNativeSetDurabilityIdentifierType(IntPtr profile, int mode);
      internal static RMWNativeSetDurabilityIdentifierType
        rmw_native_interface_set_durability =
        (RMWNativeSetDurabilityIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRMW,
        "rmw_native_interface_set_durability"),
        typeof(RMWNativeSetDurabilityIdentifierType));
  }
}
