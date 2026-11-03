# Copyright 2019-2021 Robotec.ai
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

find_package(rmw_implementation_cmake REQUIRED)

macro(accumulate_typesupports)
  set(_typesupport_impl "")
  if(${rmw_implementation}_FOUND)
    get_rmw_typesupport(_typesupport_impl ${rmw_implementation} LANGUAGE "C")
    list_append_unique(_typesupport_impls ${_typesupport_impl})
  endif()
endmacro()

macro(rosidl_generator_cs_get_typesupports TYPESUPPORT_IMPLS)
  if("$ENV{ROS_DISTRO}" STREQUAL "lyrical")
    # The generated C# loader and ROSIDL_GET_{MSG,SRV}_TYPE_SUPPORT use
    # the C dispatch layer, not an individual DDS implementation. Produce
    # one binding with that same name; ROS resolves the selected RMW.
    find_package(rosidl_typesupport_c REQUIRED)
    set(${TYPESUPPORT_IMPLS} "rosidl_typesupport_c")
  else()
    set(_typesupport_impls "")
    call_for_each_rmw_implementation(accumulate_typesupports)
    set(${TYPESUPPORT_IMPLS} ${_typesupport_impls})
  endif()
endmacro()
