using System;
using System.Runtime.InteropServices;

namespace ROS2
{
    #pragma warning disable 0169

    // rcl
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

    public struct rcl_node_options_t
    {
        private UIntPtr domain_id;
        private rcl_allocator_t allocator;
        private bool use_global_arguments;
        private rcl_arguments_t arguments;
    }

    public struct rcl_publisher_options_t
    {
        private rmw_qos_profile_t qos;
        private rcl_allocator_t allocator;
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

    // rmw
    public struct rmw_qos_profile_t
    {
        public rmw_qos_history_policy_t history;
        public ulong depth;
        public rmw_qos_reliability_policy_t reliability;
        public rmw_qos_durability_policy_t durability;
        public IntPtr deadline;
        public IntPtr lifespan;
        public rmw_qos_liveliness_policy_t liveliness;
        public IntPtr liveliness_lease_duration;
        public byte avoid_ros_namespace_conventions;
    }


    public enum rmw_qos_history_policy_t
    {
        RMW_QOS_POLICY_HISTORY_SYSTEM_DEFAULT,
        RMW_QOS_POLICY_HISTORY_KEEP_LAST,
        RMW_QOS_POLICY_HISTORY_KEEP_ALL
    }

    public enum rmw_qos_reliability_policy_t
    {
        RMW_QOS_POLICY_RELIABILITY_SYSTEM_DEFAULT,
        RMW_QOS_POLICY_RELIABILITY_RELIABLE,
        RMW_QOS_POLICY_RELIABILITY_BEST_EFFORT
    }

    public enum rmw_qos_durability_policy_t
    {
        RMW_QOS_POLICY_DURABILITY_SYSTEM_DEFAULT,
        RMW_QOS_POLICY_DURABILITY_TRANSIENT_LOCAL,
        RMW_QOS_POLICY_DURABILITY_VOLATILE
    }

    public enum rmw_qos_liveliness_policy_t
    {
        RMW_QOS_POLICY_LIVELINESS_SYSTEM_DEFAULT,
        RMW_QOS_POLICY_LIVELINESS_AUTOMATIC,
        RMW_QOS_POLICY_LIVELINESS_MANUAL_BY_NODE
    }

#pragma warning restore 0169
}
