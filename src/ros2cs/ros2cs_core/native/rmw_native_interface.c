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

#include <rmw/qos_profiles.h>
#include <rmw/types.h>
#include <rmw/rmw.h>
#include <rcl/rcl.h>

ROSIDL_GENERATOR_C_EXPORT
rmw_qos_profile_t * rmw_native_interface_create_qos_profile(int profile)
{
  enum
  {
     SENSOR_DATA,
     PARAMETERS,
     DEFAULT,
     SERVICES_DEFAULT,
     PARAMETER_EVENTS,
     SYSTEM_DEFAULT
  };

  rmw_qos_profile_t * preset_profile = (rmw_qos_profile_t *)malloc(sizeof(rmw_qos_profile_t));

  switch (profile)
  {
      case SENSOR_DATA: *preset_profile = rmw_qos_profile_sensor_data; break;
      case PARAMETERS: *preset_profile = rmw_qos_profile_parameters; break;
      case DEFAULT: *preset_profile = rmw_qos_profile_default; break;
      case SERVICES_DEFAULT: *preset_profile = rmw_qos_profile_services_default; break;
      case PARAMETER_EVENTS: *preset_profile = rmw_qos_profile_parameter_events; break;
      case SYSTEM_DEFAULT: *preset_profile = rmw_qos_profile_system_default; break;
      default: *preset_profile = rmw_qos_profile_unknown; break;
  }

  return preset_profile;
}

ROSIDL_GENERATOR_C_EXPORT
const char* rmw_native_interface_get_implementation_identifier()
{
  return rmw_get_implementation_identifier();
}

ROSIDL_GENERATOR_C_EXPORT
void rmw_native_interface_delete_qos_profile(rmw_qos_profile_t * profile)
{
  free(profile);
}

ROSIDL_GENERATOR_C_EXPORT
void rmw_native_interface_set_history(rmw_qos_profile_t * profile, int history_mode, int history_depth)
{
  profile->history = history_mode;
  profile->depth = (size_t)history_depth;
}

ROSIDL_GENERATOR_C_EXPORT
void rmw_native_interface_set_reliability(rmw_qos_profile_t * profile, int reliability_mode)
{
  profile->reliability = reliability_mode;
}

ROSIDL_GENERATOR_C_EXPORT
void rmw_native_interface_set_durability(rmw_qos_profile_t * profile, int durability_mode)
{
  profile->durability = durability_mode;
}
