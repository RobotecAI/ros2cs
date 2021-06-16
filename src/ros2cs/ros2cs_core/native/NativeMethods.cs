using System;
using System.Runtime.InteropServices;

namespace ROS2
{
    internal static class NativeMethods
    {
        private static readonly DllLoadUtils dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
        private static readonly IntPtr nativeRCL = dllLoadUtils.LoadLibraryNoSuffix("rcl");

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
        internal delegate rcl_publisher_options_t PublisherGetDefaultOptionsType();
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
        internal delegate int WaitSetInitType(ref rcl_wait_set_t wait_set,
                                              ulong number_of_subscriptions,
                                              ulong number_of_guard_conditions,
                                              ulong number_of_timers,
                                              ulong number_of_clients,
                                              ulong number_of_services,
                                              ulong number_of_events,
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
        internal delegate int WaitSetAddSubscriptionType(ref rcl_wait_set_t wait_set, ref rcl_subscription_t subscription, UIntPtr index);
        internal static WaitSetAddSubscriptionType
            rcl_wait_set_add_subscription =
            (WaitSetAddSubscriptionType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
            nativeRCL,
            "rcl_wait_set_add_subscription"),
            typeof(WaitSetAddSubscriptionType));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int WaitType(ref rcl_wait_set_t wait_set, ulong timeout);
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

        private static readonly IntPtr nativeRCUtils = dllLoadUtils.LoadLibraryNoSuffix("rcutils");

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

        private static readonly IntPtr nativeROS2CS = dllLoadUtils.LoadLibrary("ros2cs");

        internal delegate int RCLCSInitType(ref rcl_context_t context);
        internal static RCLCSInitType
            rclcs_init =
            (RCLCSInitType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
            nativeROS2CS,
            "rclcs_init"),
            typeof(RCLCSInitType));

        //TODO - load library rmw
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr GetRMWIdentifierType();
        internal static GetRMWIdentifierType
            rmw_get_implementation_identifier =
            (GetRMWIdentifierType)Marshal.GetDelegateForFunctionPointer(dllLoadUtils.GetProcAddress(
            nativeROS2CS,
            "rclcs_get_rmw_implementation_id"),
            typeof(GetRMWIdentifierType));

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
        internal delegate IntPtr SubscriptionCreateOptionsType(rmw_qos_profile_t qos);
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
        internal delegate IntPtr PublisherCreateOptionsType(rmw_qos_profile_t qos);
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
