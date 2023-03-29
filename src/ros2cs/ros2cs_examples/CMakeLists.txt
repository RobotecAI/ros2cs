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

cmake_minimum_required(VERSION 3.5)

project(ros2cs_examples C)

find_package(ament_cmake REQUIRED)
find_package(ros2cs_common REQUIRED)
find_package(ros2cs_core REQUIRED)
find_package(std_msgs REQUIRED)
find_package(sensor_msgs REQUIRED)
find_package(example_interfaces REQUIRED)
find_package(builtin_interfaces REQUIRED)
find_package(dotnet_cmake_module REQUIRED)
find_package(rosidl_generator_cs REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp6.0")
set(CSBUILD_TOOL "DotNetCore")

find_package(DotNETExtra REQUIRED)

set(_assemblies_dep_dlls
    ${ros2cs_common_ASSEMBLIES_DLL}
    ${ros2cs_core_ASSEMBLIES_DLL}
    ${std_msgs_ASSEMBLIES_DLL}
    ${sensor_msgs_ASSEMBLIES_DLL}
    ${example_interfaces_ASSEMBLIES_DLL}
    ${builtin_interfaces_ASSEMBLIES_DLL}
)

add_dotnet_executable(ros2cs_talker
  ROS2Talker.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(ros2cs_listener
  ROS2Listener.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(ros2cs_performance_talker
  ROS2PerformanceTalker.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(ros2cs_performance_listener
  ROS2PerformanceListener.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(ros2cs_client
  ROS2Client.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(ros2cs_service
  ROS2Service.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

install_dotnet(ros2cs_listener DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(ros2cs_talker DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(ros2cs_performance_talker DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(ros2cs_performance_listener DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(ros2cs_client DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(ros2cs_service DESTINATION lib/${PROJECT_NAME}/dotnet)

ament_package()
