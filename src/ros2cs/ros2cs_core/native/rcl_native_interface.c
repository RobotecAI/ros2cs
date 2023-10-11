// Copyright 2019-2023 Robotec.ai
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

#include <stdint.h>
#include <rcl/error_handling.h>
#include <rcl/context.h>
#include <rcl/node.h>
#include <rcl/publisher.h>
#include <rcl/subscription.h>
#include <rcl/service.h>
#include <rcl/client.h>
#include <rcl/guard_condition.h>
#include <rcl/wait.h>
#include <rcl/graph.h>
#include <rcl/rcl.h>
#include <rcl/time.h>
#include <rcutils/allocator.h>
#include <rcutils/strdup.h>
#include <rcutils/types.h>
#include <rmw/qos_profiles.h>

ROSIDL_GENERATOR_C_EXPORT
rcl_context_t * rclcs_get_zero_initialized_context()
{
  rcl_context_t * context = malloc(sizeof(rcl_context_t));
  *context = rcl_get_zero_initialized_context();
  return context;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_context(rcl_context_t * context)
{
  free(context);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_context_is_valid(rcl_context_t * context)
{
  // since bool has different sizes in C and C++
  if (rcl_context_is_valid(context))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
int rclcs_init(rcl_context_t *context, rcl_allocator_t allocator)
{
  rcl_init_options_t init_options = rcl_get_zero_initialized_init_options();
  rcl_ret_t ret = rcl_init_options_init(&init_options, allocator);
  if (ret != RCL_RET_OK)
  {
    return (int)ret;
  }

  ret = rcl_init(0, NULL, &init_options, context);
  if (ret != RCL_RET_OK)
  {
    return (int)ret;
  }

  ret = rcl_init_options_fini(&init_options);
  return ret;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_node_t * rclcs_get_zero_initialized_node()
{
  rcl_node_t * node = malloc(sizeof(rcl_node_t));
  *node =  rcl_get_zero_initialized_node();
  return node;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_node(rcl_node_t * node)
{
  free(node);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_node_is_valid(rcl_node_t * node)
{
  // since bool has different sizes in C and C++
  if (rcl_node_is_valid(node))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_node_options_t * rclcs_node_create_default_options()
{
  rcl_node_options_t  * default_node_options_handle = (rcl_node_options_t *)malloc(sizeof(rcl_node_options_t));
  *default_node_options_handle = rcl_node_get_default_options();
  return default_node_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_node_dispose_options(rcl_node_options_t * node_options_handle)
{
  free(node_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_subscription_t * rclcs_get_zero_initialized_subscription()
{
  rcl_subscription_t * subscription = malloc(sizeof(rcl_subscription_t));
  *subscription = rcl_get_zero_initialized_subscription();
  return subscription;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_subscription(rcl_subscription_t * subscription)
{
  free(subscription);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_subscription_is_valid(rcl_subscription_t * subscription)
{
  // since bool has different sizes in C and C++
  if (rcl_subscription_is_valid(subscription))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_subscription_options_t *rclcs_subscription_create_options(rmw_qos_profile_t * qos)
{
  rcl_subscription_options_t  * default_subscription_options_handle = (rcl_subscription_options_t *)malloc(sizeof(rcl_subscription_options_t));
  *default_subscription_options_handle = rcl_subscription_get_default_options();
  default_subscription_options_handle->qos = *qos;
  return default_subscription_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_subscription_dispose_options(rcl_subscription_options_t *subscription_options_handle)
{
  free(subscription_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_publisher_t * rclcs_get_zero_initialized_publisher()
{
  rcl_publisher_t * publisher = malloc(sizeof(rcl_publisher_t));
  *publisher = rcl_get_zero_initialized_publisher();
  return publisher;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_publisher(rcl_publisher_t * publisher)
{
  free(publisher);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_publisher_is_valid(rcl_publisher_t * publisher)
{
  // since bool has different sizes in C and C++
  if (rcl_publisher_is_valid(publisher))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_publisher_options_t *rclcs_publisher_create_options(rmw_qos_profile_t * qos)
{
  rcl_publisher_options_t *default_publisher_options_handle = (rcl_publisher_options_t *)malloc(sizeof(rcl_publisher_options_t));
  *default_publisher_options_handle = rcl_publisher_get_default_options();
  default_publisher_options_handle->qos = *qos;
  return default_publisher_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_publisher_dispose_options(rcl_publisher_options_t * publisher_options_handle)
{
  free(publisher_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_client_t * rclcs_get_zero_initialized_client()
{
  rcl_client_t * client = malloc(sizeof(rcl_client_t));
  *client = rcl_get_zero_initialized_client();
  return client;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_client(rcl_client_t * client)
{
  free(client);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_client_is_valid(rcl_client_t * client)
{
  // since bool has different sizes in C and C++
  if (rcl_client_is_valid(client))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_client_options_t *rclcs_client_create_options(rmw_qos_profile_t * qos)
{
  rcl_client_options_t *default_client_options_handle = (rcl_client_options_t *)malloc(sizeof(rcl_client_options_t));
  *default_client_options_handle = rcl_client_get_default_options();
  default_client_options_handle->qos = *qos;
  return default_client_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_client_dispose_options(rcl_client_options_t * client_options_handle)
{
  free(client_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_service_t * rclcs_get_zero_initialized_service()
{
  rcl_service_t * service = malloc(sizeof(rcl_service_t));
  *service = rcl_get_zero_initialized_service();
  return service;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_service(rcl_service_t * service)
{
  free(service);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_service_is_valid(rcl_service_t * service)
{
  // since bool has different sizes in C and C++
  if (rcl_service_is_valid(service))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_service_options_t *rclcs_service_create_options(rmw_qos_profile_t * qos)
{
  rcl_service_options_t *default_service_options_handle = (rcl_service_options_t *)malloc(sizeof(rcl_service_options_t));
  *default_service_options_handle = rcl_service_get_default_options();
  default_service_options_handle->qos = *qos;
  return default_service_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_service_dispose_options(rcl_service_options_t * service_options_handle)
{
  free(service_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_ret_t rclcs_get_guard_condition(rcl_context_t * context, rcl_guard_condition_t ** guard_condition)
{
  *guard_condition = malloc(sizeof(rcl_guard_condition_t));
  **guard_condition = rcl_get_zero_initialized_guard_condition();
  rcl_ret_t ret = rcl_guard_condition_init(*guard_condition, context, rcl_guard_condition_get_default_options());
  if (ret != RCL_RET_OK)
  {
    free(*guard_condition);
  }
  return ret;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_guard_condition(rcl_guard_condition_t * guard_condition)
{
  free(guard_condition);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_guard_condition_is_valid(rcl_guard_condition_t * guard_condition)
{
  // since there is no rcl_guard_condition_is_valid
  if (rcl_guard_condition_get_options(guard_condition) != NULL)
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
rcl_wait_set_t * rclcs_get_zero_initialized_wait_set()
{
  rcl_wait_set_t * wait_set = malloc(sizeof(rcl_wait_set_t));
  *wait_set = rcl_get_zero_initialized_wait_set();
  return wait_set;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_free_wait_set(rcl_wait_set_t * wait_set)
{
  free(wait_set);
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_wait_set_is_valid(rcl_wait_set_t * wait_set)
{
  // since bool has different sizes in C and C++
  if (rcl_wait_set_is_valid(wait_set))
  {
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_wait_set_get_subscription(rcl_wait_set_t * wait_set, size_t index, const rcl_subscription_t ** subscription)
{
  if (index < wait_set->size_of_subscriptions)
  {
    *subscription = wait_set->subscriptions[index];
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_wait_set_get_client(rcl_wait_set_t * wait_set, size_t index, const rcl_client_t ** client)
{
  if (index < wait_set->size_of_clients)
  {
    *client = wait_set->clients[index];
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_wait_set_get_service(rcl_wait_set_t * wait_set, size_t index, const rcl_service_t ** service)
{
  if (index < wait_set->size_of_services)
  {
    *service = wait_set->services[index];
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
uint8_t rclcs_wait_set_get_guard_condition(rcl_wait_set_t * wait_set, size_t index, const rcl_guard_condition_t ** guard_condition)
{
  if (index < wait_set->size_of_guard_conditions)
  {
    *guard_condition = wait_set->guard_conditions[index];
    return 1;
  }
  return 0;
}

ROSIDL_GENERATOR_C_EXPORT
char * rclcs_get_error_string()
{
  rcl_error_string_t error_string = rcl_get_error_string();
  char * error_c_string = strdup(error_string.str);
  return error_c_string;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_dispose_error_string(char * error_c_string)
{
  free(error_c_string);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_clock_t * rclcs_ros_clock_create(rcl_allocator_t * allocator_handle)
{
  rcl_clock_t  * clock_handle = (rcl_clock_t *)malloc(sizeof(rcl_clock_t));
  int32_t ret = rcl_ros_clock_init(clock_handle, allocator_handle);
  if (ret != RCL_RET_OK)
  {
    free(clock_handle);
  }
  return clock_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_ros_clock_dispose(rcl_clock_t * clock_handle)
{
  free(clock_handle);
}
