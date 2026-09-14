// SPDX-License-Identifier: Apache-2.0
// Compile-time checks for the x86_64 layouts mirrored in NativeTypes.cs.
// These checks detect size/offset drift, not all possible semantic ABI changes.
#include <stddef.h>
#include <rcl/rcl.h>
#include <rmw/types.h>
#define CHECK_SIZE(type, expected) _Static_assert(sizeof(type) == (expected), #type " layout differs from NativeTypes.cs")
#define CHECK_OFFSET(type, field, expected) _Static_assert(offsetof(type, field) == (expected), #type "." #field " offset differs from NativeTypes.cs")
CHECK_SIZE(void *, 8);
CHECK_SIZE(rcl_allocator_t, 40);
CHECK_SIZE(rcl_arguments_t, 8);
CHECK_SIZE(rcl_init_options_t, 8);
CHECK_SIZE(rcl_context_t, 24);
CHECK_OFFSET(rcl_context_t, global_arguments, 0);
CHECK_OFFSET(rcl_context_t, impl, 8);
CHECK_OFFSET(rcl_context_t, instance_id_storage, 16);
CHECK_SIZE(rcl_node_t, 16);
CHECK_SIZE(rcl_publisher_t, 8);
CHECK_SIZE(rcl_subscription_t, 8);
CHECK_SIZE(rcl_client_t, 8);
CHECK_SIZE(rcl_service_t, 8);
CHECK_SIZE(rmw_request_id_t, 24);
CHECK_OFFSET(rmw_request_id_t, sequence_number, 16);
CHECK_SIZE(rcl_wait_set_t, 104);
CHECK_OFFSET(rcl_wait_set_t, size_of_subscriptions, 8);
CHECK_OFFSET(rcl_wait_set_t, size_of_guard_conditions, 24);
CHECK_OFFSET(rcl_wait_set_t, size_of_timers, 40);
CHECK_OFFSET(rcl_wait_set_t, size_of_clients, 56);
CHECK_OFFSET(rcl_wait_set_t, size_of_services, 72);
CHECK_OFFSET(rcl_wait_set_t, size_of_events, 88);
