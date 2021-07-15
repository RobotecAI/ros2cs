using System;
using System.Runtime.InteropServices;

namespace ROS2
{
  /// <summary>
  /// A set of structures to mimic rcl structs and be able to set fields without additional native calls
  /// <description>
  /// TODO: this is a bit dangerous in that if structures change in a new ros2 version we can have
  /// a crash source. Consider removing in favor of handling all manipulation inside the native library,
  /// where structure changes will use defaults (new fields added) or produce compile-time errors (fields changed).
  /// </description>
  /// </summary>

    #pragma warning disable 0169

    public struct rcl_allocator_t
    {
        public IntPtr allocate;
        public IntPtr deallocate;
        public IntPtr reallocate;
        public IntPtr zero_allocate;
        public IntPtr state;
    }

    public struct rcl_arguments_t
    {
        private IntPtr impl;
    }

    public struct rcl_context_t
    {
        private IntPtr global_arguments;
        private IntPtr impl;
        private IntPtr instance_id_storage;
    }

    public struct rcl_error_string_t
    {
        internal IntPtr str;
    }

    public struct rcl_init_options_t
    {
        private IntPtr impl;
    }

    public struct rcl_node_t
    {
        private IntPtr context;
        private IntPtr rcl_node_impl_t;
    }

    public struct rcl_publisher_t
    {
        private IntPtr impl;
    }

    public struct rcl_subscription_t
    {
        private IntPtr impl;
    }

    public struct rcl_wait_set_t
    {
        private IntPtr subscriptions;
        private UIntPtr size_of_subscriptions;
        private IntPtr guard_conditions;
        private UIntPtr size_of_guard_conditions;
        private IntPtr timers;
        private UIntPtr size_of_timers;
        private IntPtr clients;
        private UIntPtr size_of_clients;
        private IntPtr services;
        private UIntPtr size_of_services;
        private IntPtr events;
        private UIntPtr size_of_events;
        private IntPtr impl;
    }

    public struct rcl_clock_t
    {
        private int type;
        private IntPtr jump_callbacks;
        private UIntPtr num_jump_callbacks;
        private IntPtr get_now;
        private IntPtr data;
        rcl_allocator_t allocator;
    }

#pragma warning restore 0169
}
