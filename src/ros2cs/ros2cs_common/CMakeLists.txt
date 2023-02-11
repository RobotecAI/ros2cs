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

project(ros2cs_common)

find_package(ament_cmake REQUIRED)
find_package(ament_cmake_export_assemblies REQUIRED)
find_package(dotnet_cmake_module REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netstandard2.0")
set(CSBUILD_TOOL "DotNetCore")

find_package(DotNETExtra REQUIRED)

set(CS_INTERFACES
  Message.cs
  DllLoadUtils.cs
  Exceptions.cs
  Ros2csLogger.cs
  MessageInternals.cs
  Benchmark.cs
  interfaces/IExtendedDisposable.cs
  properties/AssemblyInfo.cs
)

add_dotnet_library(${PROJECT_NAME}
SOURCES
  ${CS_INTERFACES}
  INCLUDE_DLLS
  ${_assembly_deps_dll}
)

install_dotnet(${PROJECT_NAME} DESTINATION lib/dotnet)
ament_export_assemblies_dll("lib/dotnet/${PROJECT_NAME}.dll")
ament_export_dependencies(ament_cmake)

if(BUILD_TESTING)
  find_package(ament_lint_auto REQUIRED)
  ament_lint_auto_find_test_dependencies()
endif()

ament_package()
