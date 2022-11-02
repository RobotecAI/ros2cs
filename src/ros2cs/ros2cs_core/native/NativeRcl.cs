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
  /// An internal class to manage all (unmodified) native calls to rcl and rcutils
  /// </summary>
  internal static class NativeRcl
  {
    private static readonly DllLoadUtils dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
    private static readonly IntPtr nativeRCL = dllLoadUtils.LoadLibraryNoSuffix("rcl");
    private static readonly IntPtr nativeRCUtils = dllLoadUtils.LoadLibraryNoSuffix("rcutils");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_context_t GetZeroInitializedContextType();
    internal static GetZeroInitializedContextType
        rcl_get_zero_initialized_context =
        (GetZeroInitializedContextType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_context"),
        typeof(GetZeroInitializedContextType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_init_options_t GetZeroInitializedInitOptionsType();
    internal static GetZeroInitializedInitOptionsType
        rcl_get_zero_initialized_init_options =
        (GetZeroInitializedInitOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_init_options"),
        typeof(GetZeroInitializedInitOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int InitOptionsInitType(ref rcl_init_options_t init_options, rcl_allocator_t allocator);
    internal static InitOptionsInitType
    rcl_init_options_init =
    (InitOptionsInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
    nativeRCL,
    "rcl_init_options_init"),
    typeof(InitOptionsInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ShutdownType(ref rcl_context_t context);
    internal static ShutdownType
        rcl_shutdown =
        (ShutdownType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_shutdown"),
        typeof(ShutdownType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool ContextIsValidType(ref rcl_context_t context);
    internal static ContextIsValidType
        rcl_context_is_valid =
        (ContextIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_context_is_valid"),
        typeof(ContextIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int InitType(int argc, [In, Out] string[] argv, ref rcl_init_options_t option, ref rcl_context_t context);
    internal static InitType
        rcl_init =
        (InitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_init"),
        typeof(InitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ContextFiniType(ref rcl_context_t context);
    internal static ContextFiniType
        rcl_context_fini =
        (ContextFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_context_fini"),
        typeof(ContextFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_node_t GetZeroInitializedNodeType();
    internal static GetZeroInitializedNodeType
        rcl_get_zero_initialized_node =
        (GetZeroInitializedNodeType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_node"),
        typeof(GetZeroInitializedNodeType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NodeInitType(ref rcl_node_t node, string name, string node_namespace, ref rcl_context_t context, IntPtr default_options);
    internal static NodeInitType
        rcl_node_init =
        (NodeInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_node_init"),
        typeof(NodeInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NodeFiniType(ref rcl_node_t node);
    internal static NodeFiniType
        rcl_node_fini =
        (NodeFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_node_fini"),
        typeof(NodeFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr NodeGetNameType(ref rcl_node_t node);
    internal static NodeGetNameType
        rcl_node_get_name =
        (NodeGetNameType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_node_get_name"),
        typeof(NodeGetNameType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr NodeGetNamespaceType(ref rcl_node_t node);
    internal static NodeGetNamespaceType
        rcl_node_get_namespace =
        (NodeGetNamespaceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_node_get_namespace"),
        typeof(NodeGetNamespaceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr ClientGetDefaultOptionsType();
    internal static ClientGetDefaultOptionsType
        rcl_client_get_default_options =
        (ClientGetDefaultOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_client_get_default_options"),
        typeof(ClientGetDefaultOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_client_t GetZeroInitiazizedClientType();
    internal static GetZeroInitiazizedClientType
        rcl_get_zero_initialized_client =
        (GetZeroInitiazizedClientType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_client"),
        typeof(GetZeroInitiazizedClientType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ClientInitType(ref rcl_client_t client, ref rcl_node_t node, IntPtr type_support_ptr, string topic_name, IntPtr client_options);
    internal static ClientInitType
        rcl_client_init =
        (ClientInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_client_init"),
        typeof(ClientInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ClientFiniType(ref rcl_client_t client, ref rcl_node_t node);
    internal static ClientFiniType
        rcl_client_fini =
        (ClientFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_client_fini"),
        typeof(ClientFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SendRequestType(ref rcl_client_t client, IntPtr message, ref long sequence_number);
    internal static SendRequestType
        rcl_send_request =
        (SendRequestType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_send_request"),
        typeof(SendRequestType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int TakeResponceType(ref rcl_client_t client, ref rcl_rmw_request_id_t request_header, IntPtr ros_response);
    internal static TakeResponceType
        rcl_take_response =
        (TakeResponceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_take_response"),
        typeof(TakeResponceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ServiceIsAvailableType(ref rcl_node_t node, ref rcl_client_t client, ref bool is_available);
    internal static ServiceIsAvailableType
        rcl_service_server_is_available =
        (ServiceIsAvailableType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_service_server_is_available"),
        typeof(ServiceIsAvailableType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr ServiceGetDefaultOptionsType();
    internal static ServiceGetDefaultOptionsType
        rcl_service_get_default_options =
        (ServiceGetDefaultOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_service_get_default_options"),
        typeof(ServiceGetDefaultOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_service_t GetZeroInitiazizedServiceType();
    internal static GetZeroInitiazizedServiceType
        rcl_get_zero_initialized_service =
        (GetZeroInitiazizedServiceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_service"),
        typeof(GetZeroInitiazizedServiceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ServiceInitType(ref rcl_service_t service, ref rcl_node_t node, IntPtr type_support_ptr, string topic_name, IntPtr service_options);
    internal static ServiceInitType
        rcl_service_init =
        (ServiceInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_service_init"),
        typeof(ServiceInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int ServiceFiniType(ref rcl_service_t client, ref rcl_node_t node);
    internal static ServiceFiniType
        rcl_service_fini =
        (ServiceFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_service_fini"),
        typeof(ServiceFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int TakeRequestType(ref rcl_service_t service, ref rcl_rmw_request_id_t request_header, IntPtr message_handle);
    internal static TakeRequestType
        rcl_take_request =
        (TakeRequestType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_take_request"),
        typeof(TakeRequestType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SendResponceType( ref rcl_service_t service, ref rcl_rmw_request_id_t request_header, IntPtr responce_info);
    ///internal delegate int SendResponceType( ref rcl_service_t service, ref rcl_rmw_request_id_t request_header, ref IntPtr responce_info);
    internal static SendResponceType
        rcl_send_response =
        (SendResponceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_send_response"),
        typeof(SendResponceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr PublisherGetDefaultOptionsType();
    internal static PublisherGetDefaultOptionsType
        rcl_publisher_get_default_options =
        (PublisherGetDefaultOptionsType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_publisher_get_default_options"),
        typeof(PublisherGetDefaultOptionsType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_publisher_t GetZeroInitiazizedPublisherType();
    internal static GetZeroInitiazizedPublisherType
        rcl_get_zero_initialized_publisher =
        (GetZeroInitiazizedPublisherType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_publisher"),
        typeof(GetZeroInitiazizedPublisherType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int PublisherInitType(ref rcl_publisher_t publisher, ref rcl_node_t node, IntPtr type_support_ptr, string topic_name, IntPtr publisher_options);
    internal static PublisherInitType
        rcl_publisher_init =
        (PublisherInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_publisher_init"),
        typeof(PublisherInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int PublisherFiniType(ref rcl_publisher_t publisher, ref rcl_node_t node);
    internal static PublisherFiniType
        rcl_publisher_fini =
        (PublisherFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_publisher_fini"),
        typeof(PublisherFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int PublishType(ref rcl_publisher_t publisher, IntPtr message, IntPtr allocator);
    internal static PublishType
        rcl_publish =
        (PublishType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_publish"),
        typeof(PublishType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_subscription_t GetZeroInitializedSubcriptionType();
    internal static GetZeroInitializedSubcriptionType
        rcl_get_zero_initialized_subscription =
        (GetZeroInitializedSubcriptionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_subscription"),
        typeof(GetZeroInitializedSubcriptionType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SubscriptionInitType(ref rcl_subscription_t subscription, ref rcl_node_t node, IntPtr type_support_ptr, string topic_name, IntPtr subscription_options);
    internal static SubscriptionInitType
        rcl_subscription_init =
        (SubscriptionInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_subscription_init"),
        typeof(SubscriptionInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SubscriptionFiniType(ref rcl_subscription_t subscription, ref rcl_node_t node);
    internal static SubscriptionFiniType
        rcl_subscription_fini =
        (SubscriptionFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_subscription_fini"),
        typeof(SubscriptionFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool SubscriptionIsValidType(ref rcl_subscription_t subscription);
    internal static SubscriptionIsValidType
        rcl_subscription_is_valid =
        (SubscriptionIsValidType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_subscription_is_valid"),
        typeof(SubscriptionIsValidType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int TakeType(ref rcl_subscription_t subscription, IntPtr message_handle, IntPtr message_info, IntPtr allocation);
    internal static TakeType
        rcl_take =
        (TakeType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_take"),
        typeof(TakeType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_wait_set_t GetZeroInitializedWaitSetType();
    internal static GetZeroInitializedWaitSetType
        rcl_get_zero_initialized_wait_set =
        (GetZeroInitializedWaitSetType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_get_zero_initialized_wait_set"),
        typeof(GetZeroInitializedWaitSetType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetResizeType(ref rcl_wait_set_t wait_set,
                                            UIntPtr number_of_subscriptions,
                                            UIntPtr number_of_guard_conditions,
                                            UIntPtr number_of_timers,
                                            UIntPtr number_of_clients,
                                            UIntPtr number_of_services,
                                            UIntPtr number_of_events);
    internal static WaitSetResizeType rcl_wait_set_resize =
        (WaitSetResizeType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_resize"),
        typeof(WaitSetResizeType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetInitType(ref rcl_wait_set_t wait_set,
                                          UIntPtr number_of_subscriptions,
                                          UIntPtr number_of_guard_conditions,
                                          UIntPtr number_of_timers,
                                          UIntPtr number_of_clients,
                                          UIntPtr number_of_services,
                                          UIntPtr number_of_events,
                                          ref rcl_context_t context,
                                          rcl_allocator_t allocator);
    internal static WaitSetInitType
        rcl_wait_set_init =
        (WaitSetInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_init"),
        typeof(WaitSetInitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WatiSetFiniType(ref rcl_wait_set_t wait_set);
    internal static WatiSetFiniType
        rcl_wait_set_fini =
        (WatiSetFiniType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_fini"),
        typeof(WatiSetFiniType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetClearType(ref rcl_wait_set_t wait_set);
    internal static WaitSetClearType
        rcl_wait_set_clear =
        (WaitSetClearType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_clear"),
        typeof(WaitSetClearType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetAddSubscriptionType(ref rcl_wait_set_t wait_set, ref rcl_subscription_t subscription, ref UIntPtr index);
    internal static WaitSetAddSubscriptionType
        rcl_wait_set_add_subscription =
        (WaitSetAddSubscriptionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_add_subscription"),
        typeof(WaitSetAddSubscriptionType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetAddClientType(ref rcl_wait_set_t wait_set, ref rcl_client_t client, ref UIntPtr index);
    internal static WaitSetAddClientType
        rcl_wait_set_add_client =
        (WaitSetAddClientType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_add_client"),
        typeof(WaitSetAddClientType));
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitSetAddServiceType(ref rcl_wait_set_t wait_set, ref rcl_service_t service, ref UIntPtr index);
    internal static WaitSetAddServiceType
        rcl_wait_set_add_service =
        (WaitSetAddServiceType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait_set_add_service"),
        typeof(WaitSetAddServiceType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int WaitType(ref rcl_wait_set_t wait_set, long timeout);
    internal static WaitType
        rcl_wait =
        (WaitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_wait"),
        typeof(WaitType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int RclClockGetNow(IntPtr ros_clock, ref long query_now);
    internal static RclClockGetNow
        rcl_clock_get_now =
        (RclClockGetNow)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCL,
        "rcl_clock_get_now"),
        typeof(RclClockGetNow));

    // rcutils

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate rcl_allocator_t RclGetDefaultAllocatorType();
    internal static RclGetDefaultAllocatorType
        rcutils_get_default_allocator =
        (RclGetDefaultAllocatorType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCUtils,
        "rcutils_get_default_allocator"),
        typeof(RclGetDefaultAllocatorType));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ResetErrorType();
    internal static ResetErrorType
        rcl_reset_error =
        (ResetErrorType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
        nativeRCUtils,
        "rcutils_reset_error"),
        typeof(ResetErrorType));
  }
}
