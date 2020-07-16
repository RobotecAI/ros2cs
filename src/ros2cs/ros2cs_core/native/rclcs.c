#define nullptr ((void*)0)

#include <rcl/error_handling.h>
#include <rcl/expand_topic_name.h>
#include <rcl/graph.h>
#include <rcl/node.h>
#include <rcl/rcl.h>
#include <rcl/time.h>
#include <rcl/validate_topic_name.h>
#include <rcl_interfaces/msg/parameter_type.h>
#include <rcutils/allocator.h>
#include <rcutils/format_string.h>
#include <rcutils/strdup.h>
#include <rcutils/types.h>
#include <rmw/error_handling.h>
#include <rmw/rmw.h>
#include <rmw/serialized_message.h>
#include <rmw/types.h>
#include <rmw/validate_full_topic_name.h>
#include <rmw/validate_namespace.h>
#include <rmw/validate_node_name.h>
#include <rmw/qos_profiles.h>
#include <signal.h>

ROSIDL_GENERATOR_C_EXPORT
int rclcs_init(rcl_context_t *context, rcl_allocator_t allocator)
{
  rcl_init_options_t init_options = rcl_get_zero_initialized_init_options();

  rcl_ret_t ret = rcl_init_options_init(&init_options, allocator);
  if (ret != RCL_RET_OK)
    return (int)ret;

  ret = rcl_init(0, NULL, &init_options, context);
  if (ret != RCL_RET_OK)
    return (int)ret;

  ret = rcl_init_options_fini(&init_options);
  return ret;
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
rcl_subscription_options_t *rclcs_subscription_create_options(rmw_qos_profile_t qos)
{
  rcl_subscription_options_t  * default_subscription_options_handle = (rcl_subscription_options_t *)malloc(sizeof(rcl_subscription_options_t));
  *default_subscription_options_handle = rcl_subscription_get_default_options();
  default_subscription_options_handle->qos = qos;
  return default_subscription_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_subscription_dispose_options(rcl_subscription_options_t *subscription_options_handle)
{
  free(subscription_options_handle);
}

ROSIDL_GENERATOR_C_EXPORT
rcl_publisher_options_t *rclcs_publisher_create_options(rmw_qos_profile_t qos)
{
  rcl_publisher_options_t *default_publisher_options_handle = (rcl_publisher_options_t *)malloc(sizeof(rcl_publisher_options_t));
  *default_publisher_options_handle = rcl_publisher_get_default_options();
  default_publisher_options_handle->qos = qos;
  return default_publisher_options_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_publisher_dispose_options(rcl_publisher_options_t * publisher_options_handle)
{
  free(publisher_options_handle);
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
    free(clock_handle);
  return clock_handle;
}

ROSIDL_GENERATOR_C_EXPORT
void rclcs_ros_clock_dispose(rcl_clock_t * clock_handle)
{
  free(clock_handle);
}

const char *rclcs_get_rmw_implementation_id()
{
  const char *id_string = rmw_get_implementation_identifier();
  return id_string;
}
