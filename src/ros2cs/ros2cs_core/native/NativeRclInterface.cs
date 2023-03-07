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
  /// An internal class to manage all wrapped native calls to rcl and rcutils.
  /// These are wrapped usually for one of two reasons:
  /// 1. Packaging several rcl calls into one (e. g. rclcs_init)
  /// 2. Tracking / handling of unmanaged memory that is allocated in the native library
  /// </summary>
  internal static class NativeRclInterface
  {
    private static readonly DllLoadUtils dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
    private static readonly IntPtr nativeROS2CS = dllLoadUtils.LoadLibrary("ros2cs");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedContextType();
    internal static GetZeroInitializedContextType
        rclcs_get_zero_initialized_context =
        (GetZeroInitializedContextType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_context"),
        typeof(GetZeroInitializedContextType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeContextType(IntPtr context);
    internal static FreeContextType
        rclcs_free_context =
        (FreeContextType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_context"),
        typeof(FreeContextType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool ContextIsValidType(IntPtr context);
    internal static ContextIsValidType
        rclcs_context_is_valid =
        (ContextIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_context_is_valid"),
        typeof(ContextIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int RCLCSInitType(IntPtr context, rcl_allocator_t allocator);
    internal static RCLCSInitType
        rclcs_init =
        (RCLCSInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_init"),
        typeof(RCLCSInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedNodeType();
    internal static GetZeroInitializedNodeType
        rclcs_get_zero_initialized_node =
        (GetZeroInitializedNodeType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_node"),
        typeof(GetZeroInitializedNodeType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeNodeType(IntPtr node);
    internal static FreeNodeType
        rclcs_free_node =
        (FreeNodeType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_node"),
        typeof(FreeNodeType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool NodeIsValidType(IntPtr node);
    internal static NodeIsValidType
        rclcs_node_is_valid =
        (NodeIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_node_is_valid"),
        typeof(NodeIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetErrorStringType();
    internal static GetErrorStringType
        rclcs_get_error_string =
        (GetErrorStringType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_error_string"),
        typeof(GetErrorStringType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DisposeErrorStringType(IntPtr error_c_string);
    internal static DisposeErrorStringType
        rclcs_dispose_error_string =
        (DisposeErrorStringType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_dispose_error_string"),
        typeof(DisposeErrorStringType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr NodeCreateDefaltOptionsType();
    internal static NodeCreateDefaltOptionsType
        rclcs_node_create_default_options =
        (NodeCreateDefaltOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_node_create_default_options"),
        typeof(NodeCreateDefaltOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void NodeDisposeOptionsType(IntPtr options);
    internal static NodeDisposeOptionsType
        rclcs_node_dispose_options =
        (NodeDisposeOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_node_dispose_options"),
        typeof(NodeDisposeOptionsType));

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedSubscriptionType();
    internal static GetZeroInitializedSubscriptionType
        rclcs_get_zero_initialized_subscription =
        (GetZeroInitializedSubscriptionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_subscription"),
        typeof(GetZeroInitializedSubscriptionType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeSubscriptionType(IntPtr subscription);
    internal static FreeSubscriptionType
        rclcs_free_subscription =
        (FreeSubscriptionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_subscription"),
        typeof(FreeSubscriptionType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool SubscriptionIsValidType(IntPtr subscription);
    internal static SubscriptionIsValidType
        rclcs_subscription_is_valid =
        (SubscriptionIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_subscription_is_valid"),
        typeof(SubscriptionIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr SubscriptionCreateOptionsType(IntPtr qos);
    internal static SubscriptionCreateOptionsType
        rclcs_subscription_create_options =
        (SubscriptionCreateOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_subscription_create_options"),
        typeof(SubscriptionCreateOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SubscriptionDisposeOptionsType(IntPtr options);
    internal static SubscriptionDisposeOptionsType
        rclcs_subscription_dispose_options =
        (SubscriptionDisposeOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_subscription_dispose_options"),
        typeof(SubscriptionDisposeOptionsType));

   [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedPublisherType();
    internal static GetZeroInitializedPublisherType
        rclcs_get_zero_initialized_publisher =
        (GetZeroInitializedPublisherType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_publisher"),
        typeof(GetZeroInitializedPublisherType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreePublisherType(IntPtr publisher);
    internal static FreePublisherType
        rclcs_free_publisher =
        (FreePublisherType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_publisher"),
        typeof(FreePublisherType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool PublisherIsValidType(IntPtr publisher);
    internal static PublisherIsValidType
        rclcs_publisher_is_valid =
        (PublisherIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_publisher_is_valid"),
        typeof(PublisherIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr PublisherCreateOptionsType(IntPtr qos);
    internal static PublisherCreateOptionsType
        rclcs_publisher_create_options =
        (PublisherCreateOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_publisher_create_options"),
        typeof(PublisherCreateOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PublisherDisposeOptionsType(IntPtr options);
    internal static PublisherDisposeOptionsType
        rclcs_publisher_dispose_options =
        (PublisherDisposeOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_publisher_dispose_options"),
        typeof(PublisherDisposeOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedClientType();
    internal static GetZeroInitializedClientType
        rclcs_get_zero_initialized_client =
        (GetZeroInitializedClientType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_client"),
        typeof(GetZeroInitializedClientType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeClientType(IntPtr client);
    internal static FreeClientType
        rclcs_free_client =
        (FreeClientType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_client"),
        typeof(FreeClientType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool ClientIsValidType(IntPtr client);
    internal static ClientIsValidType
        rclcs_client_is_valid =
        (ClientIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_client_is_valid"),
        typeof(ClientIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr ClientCreateOptionsType(IntPtr qos);
    internal static ClientCreateOptionsType
        rclcs_client_create_options =
        (ClientCreateOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_client_create_options"),
        typeof(ClientCreateOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ClientDisposeOptionsType(IntPtr options);
    internal static ClientDisposeOptionsType
        rclcs_client_dispose_options =
        (ClientDisposeOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_client_dispose_options"),
        typeof(ClientDisposeOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedServiceType();
    internal static GetZeroInitializedServiceType
        rclcs_get_zero_initialized_service =
        (GetZeroInitializedServiceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_service"),
        typeof(GetZeroInitializedServiceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeServiceType(IntPtr node);
    internal static FreeServiceType
        rclcs_free_service =
        (FreeServiceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_service"),
        typeof(FreeServiceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool ServiceIsValidType(IntPtr service);
    internal static ServiceIsValidType
        rclcs_service_is_valid =
        (ServiceIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_service_is_valid"),
        typeof(ServiceIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int GuardConditionInitType(IntPtr context, out IntPtr guard_condition);
    internal static GuardConditionInitType
        rclcs_get_guard_condition =
        (GuardConditionInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_guard_condition"),
        typeof(GuardConditionInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeGuardConditionType(IntPtr guard_condition);
    internal static FreeGuardConditionType
        rclcs_free_guard_condition =
        (FreeGuardConditionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_guard_condition"),
        typeof(FreeGuardConditionType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool GuardConditionIsValidType(IntPtr guard_condition);
    internal static GuardConditionIsValidType
        rclcs_guard_condition_is_valid =
        (GuardConditionIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_guard_condition_is_valid"),
        typeof(GuardConditionIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr GetZeroInitializedWaitSetType();
    internal static GetZeroInitializedWaitSetType
        rclcs_get_zero_initialized_wait_set =
        (GetZeroInitializedWaitSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_get_zero_initialized_wait_set"),
        typeof(GetZeroInitializedWaitSetType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FreeWaitSetType(IntPtr waitSet);
    internal static FreeWaitSetType
        rclcs_free_wait_set =
        (FreeWaitSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_free_wait_set"),
        typeof(FreeWaitSetType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool WaitSetIsValidType(IntPtr waitSet);
    internal static WaitSetIsValidType
        rclcs_wait_set_is_valid =
        (WaitSetIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_is_valid"),
        typeof(WaitSetIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool WaitSetGetType(IntPtr waitSet, UIntPtr index, out IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal delegate bool WaitSetSetType(IntPtr waitSet, UIntPtr index, IntPtr value);

    internal static WaitSetGetType
        rclcs_wait_set_get_subscription =
        (WaitSetGetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_get_subscription"),
        typeof(WaitSetGetType));

    internal static WaitSetSetType
        rclcs_wait_set_set_subscription =
        (WaitSetSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_set_subscription"),
        typeof(WaitSetSetType));

    internal static WaitSetGetType
        rclcs_wait_set_get_client =
        (WaitSetGetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_get_client"),
        typeof(WaitSetGetType));

    internal static WaitSetSetType
        rclcs_wait_set_set_client =
        (WaitSetSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_set_client"),
        typeof(WaitSetSetType));

    internal static WaitSetGetType
        rclcs_wait_set_get_service =
        (WaitSetGetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_get_service"),
        typeof(WaitSetGetType));

    internal static WaitSetSetType
        rclcs_wait_set_set_service =
        (WaitSetSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_set_service"),
        typeof(WaitSetSetType));

    internal static WaitSetGetType
        rclcs_wait_set_get_guard_condition =
        (WaitSetGetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_get_guard_condition"),
        typeof(WaitSetGetType));

    internal static WaitSetSetType
        rclcs_wait_set_set_guard_condition =
        (WaitSetSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_wait_set_set_guard_condition"),
        typeof(WaitSetSetType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr ServiceCreateOptionsType(IntPtr qos);
    internal static ServiceCreateOptionsType
        rclcs_service_create_options =
        (ServiceCreateOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_service_create_options"),
        typeof(ServiceCreateOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ServiceDisposeOptionsType(IntPtr options);
    internal static ServiceDisposeOptionsType
        rclcs_service_dispose_options =
        (ServiceDisposeOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_service_dispose_options"),
        typeof(ServiceDisposeOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr RclcsClockCreate(ref rcl_allocator_t allocator_handle);
    internal static RclcsClockCreate
        rclcs_ros_clock_create =
        (RclcsClockCreate)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_ros_clock_create"),
        typeof(RclcsClockCreate));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RclcsClockDispose(IntPtr clock_handle);
    internal static RclcsClockDispose
        rclcs_ros_clock_dispose =
        (RclcsClockDispose)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeROS2CS,
        "rclcs_ros_clock_dispose"),
        typeof(RclcsClockDispose));
  }
}
