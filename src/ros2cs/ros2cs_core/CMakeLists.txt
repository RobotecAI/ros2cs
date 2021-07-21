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

cmake_minimum_required(VERSION 3.6)

project(ros2cs_core C)

# Default to C11
if(NOT CMAKE_C_STANDARD)
  set(CMAKE_C_STANDARD 11)
endif()
if(CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID MATCHES "Clang")
  add_compile_options(-Wall -Wextra)
endif()

find_package(ros2cs_common REQUIRED)
find_package(ament_cmake_export_assemblies REQUIRED)
find_package(ament_cmake REQUIRED)
find_package(rcl REQUIRED)
find_package(rcl_interfaces REQUIRED)
find_package(rmw REQUIRED)
find_package(rcutils REQUIRED)
find_package(rmw_implementation REQUIRED)
find_package(rmw_implementation_cmake REQUIRED)
find_package(rosidl_generator_c REQUIRED)
find_package(dotnet_cmake_module REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
find_package(DotNETExtra REQUIRED)

# Only build the library if a C typesupport exists
get_rmw_typesupport(typesupport_impls "rmw_implementation" LANGUAGE "c")
if(typesupport_impls STREQUAL "")
  message(STATUS "Skipping rclcs because no C typesupport library was found.")
  return()
endif()

function(configure_csharp_c_extension_library _library_name)
  install(TARGETS ${_library_name} EXPORT ${_library_name}
    ARCHIVE DESTINATION lib
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin
  )
endfunction()

add_library(
  ros2cs_native SHARED
  native/rcl_native_interface.c
  native/rmw_native_interface.c
)

ament_target_dependencies(ros2cs_native
  "builtin_interfaces"
  "rcl"
  "rosidl_generator_c"
  "rosidl_typesupport_c"
  "rcl_yaml_param_parser"
  "rcutils"
)
ament_export_libraries(ros2cs_native)
ament_export_targets(ros2cs_native)
configure_csharp_c_extension_library(ros2cs_native)

set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")

set(CS_INTERFACES
  interfaces/INode.cs
  interfaces/IPublisher.cs
  interfaces/ISubscription.cs
)

set(CS_NATIVE
  native/RCLRet.cs
  native/NativeRcl.cs
  native/NativeRclInterface.cs
  native/NativeRmwInterface.cs
  native/NativeTypes.cs
)

set(CS_UTILS
  utils/Utils.cs
)

set(CS_SOURCES
  ${CS_INTERFACES}
  ${CS_NATIVE}
  ${CS_UTILS}
  Clock.cs
  Node.cs
  Publisher.cs
  QualityOfServiceProfile.cs
  Ros2cs.cs
  Subscription.cs
  WaitSet.cs
  properties/AssemblyInfo.cs
)

set(_assembly_deps_dll "")
foreach(_assembly_dep ${ros2cs_common_ASSEMBLIES_DLL})
  list(APPEND _assembly_deps_dll "${_assembly_dep}")
endforeach()

add_dotnet_library(${PROJECT_NAME}
SOURCES
  ${CS_SOURCES}
  INCLUDE_DLLS
  ${_assembly_deps_dll}
)

install_dotnet(${PROJECT_NAME} DESTINATION lib/dotnet)
ament_export_assemblies_dll("lib/dotnet/${PROJECT_NAME}.dll")

ament_export_dependencies(ros2cs_common)
ament_export_dependencies(ament_cmake)
ament_export_dependencies(builtin_interfaces)
ament_export_dependencies(rcl)
ament_export_dependencies(rosidl_generator_c)
ament_export_dependencies(rosidl_typesupport_c)

option(STANDALONE_BUILD "Deploy standalone libraries with build" ON)

if(STANDALONE_BUILD)
  set(REQ_STANDALONE_LIBS "")
  set(REQ_THIRD_PARTY_STANDALONE_LIBS "")
  set(REQ_STANDALONE_DLLS "")

  macro(get_standalone_third_party_dependencies _library_name)
    find_file(${_library_name}_PATH "${_library_name}")
    if("${${_library_name}_PATH}" STREQUAL "${_library_name}_PATH-NOTFOUND")
      message( FATAL_ERROR "Can't find third party dependency: ${_library_name}" )
    endif()

    list(APPEND REQ_THIRD_PARTY_STANDALONE_LIBS ${${_library_name}_PATH})
  endmacro()

  macro(install_standalone_third_party)
    install(FILES ${REQ_THIRD_PARTY_STANDALONE_LIBS}
      DESTINATION standalone
    )
  endmacro()

  macro(get_standalone_dependencies _library_name)
    find_package(${_library_name} REQUIRED)

    # Get rmw_dds_common typesupports for dds implementations
    if("${_library_name}" STREQUAL "rmw_dds_common")
      ament_index_get_resources(middleware_implementations "rmw_typesupport")
      foreach(rmw_impl ${middleware_implementations})
        string(REGEX REPLACE "rmw_" "" rmw_impl ${rmw_impl})
        list(APPEND REQ_STANDALONE_LIBS ${rmw_dds_common_LIBRARIES__rosidl_typesupport_${rmw_impl}})
      endforeach()
    endif()

    # Get cyclonedds DDSC
    if("${_library_name}" STREQUAL "CycloneDDS")
      if (WIN32)
        get_target_property(DDSC_LIB_PATH CycloneDDS::ddsc IMPORTED_IMPLIB_RELWITHDEBINFO)
      elseif(UNIX)
        get_target_property(DDSC_LIB_PATH CycloneDDS::ddsc IMPORTED_LOCATION_NONE)
      endif()
      list(APPEND REQ_STANDALONE_LIBS ${DDSC_LIB_PATH})
    endif()

    list(APPEND REQ_STANDALONE_LIBS ${${_library_name}_LIBRARIES})
  endmacro()

  macro(install_standalone_dependencies)
    # Filter valid libraries
    list(FILTER REQ_STANDALONE_LIBS INCLUDE REGEX ".*(lib|dll|so)(\.[0-9])*$")
    list(REMOVE_DUPLICATES REQ_STANDALONE_LIBS)

    if (WIN32)
      foreach(lib_path ${REQ_STANDALONE_LIBS})
        string(REGEX REPLACE "\/(l|L)ib\/" "/bin/" bin_path ${lib_path})
        string(REGEX REPLACE "\.lib$" ".dll" dll_path ${bin_path})
        list(APPEND REQ_STANDALONE_DLLS ${dll_path})
      endforeach()
      install(FILES ${REQ_STANDALONE_DLLS}
        DESTINATION standalone
      )

      # There is an issue with resolving environment variable paths in runtime for "rcpputils" library on Windows OS.
      # We replacing this library with our patched version. See "resources/rcpputils.diff".
      install(FILES "${CMAKE_CURRENT_BINARY_DIR}/../../resources/Windows/x86_64/rcpputils.dll"
        DESTINATION standalone
      )
    elseif(UNIX)
      set(_resolvedFiles "")
      foreach(lib_path ${REQ_STANDALONE_LIBS})
        # Generate soversion files (.so.<major>). Some libs links to soversion symlinks which are not returned by find_package by default.
        get_filename_component(_resolvedFile "${lib_path}" REALPATH)
        string(REGEX REPLACE "so(\.[0-9]*)(\.[0-9]*)(\.[0-9]*)$" "so\\1" soversion_lib "${_resolvedFile}")
        get_filename_component(_resolved_soversion "${soversion_lib}" REALPATH)
        list (APPEND _resolvedFiles "${_resolvedFile}")

        # If soversion file can be created, then install library with soversion name.
        if(NOT "${soversion_lib}" STREQUAL "${_resolved_soversion}")
          get_filename_component(soversion_lib_name "${soversion_lib}" NAME)
          install(FILES ${_resolved_soversion}
            DESTINATION standalone
            RENAME ${soversion_lib_name}
          )
        endif()
      endforeach()

      install(FILES ${_resolvedFiles}
        DESTINATION standalone
      )
    endif()
  endmacro()

  set(ros2_standalone_libs
    rcl
    rcl_logging_spdlog
    rcl_yaml_param_parser
    rcpputils
    rcutils
    rmw
    CycloneDDS
    rmw_cyclonedds_cpp
    FastRTPS
    rmw_fastrtps_cpp
    rmw_dds_common
    rmw_implementation
    rosidl_runtime_c
    rosidl_typesupport_c
    rosidl_typesupport_cpp
    rosidl_typesupport_introspection_c
    rosidl_typesupport_introspection_cpp
    spdlog
    tracetools
    yaml
  )

  foreach(ros2_standalone_lib ${ros2_standalone_libs})
    get_standalone_dependencies(${ros2_standalone_lib})
  endforeach()
  install_standalone_dependencies()

  if(WIN32)
    set(third_party_standalone_libs
      libssl-1_1-x64.dll
      libcrypto-1_1-x64.dll
      msvcp140.dll
      vcruntime140.dll
      vcruntime140_1.dll
      tinyxml2.dll
    )
    foreach(third_party_lib ${third_party_standalone_libs})
      get_standalone_third_party_dependencies(${third_party_lib})
    endforeach()
    install_standalone_third_party()
  endif(WIN32)

endif()

# Tests
if(BUILD_TESTING)
  find_package(std_msgs REQUIRED)
  find_package(test_msgs REQUIRED)

  set(DOTNET_OUTPUT_PATH
    ${CSHARP_BUILDER_OUTPUT_PATH}/${CSHARP_TARGET_FRAMEWORK}/${DOTNET_CORE_RUNTIME}
  )

  set(ASSEMBLIES_DEP_DLLS
      ${DOTNET_OUTPUT_PATH}"/ros2cs_core.dll"
      ${ros2cs_common_ASSEMBLIES_DLL}
      ${std_msgs_ASSEMBLIES_DLL}
      ${test_msgs_ASSEMBLIES_DLL}
      ${rosidl_generator_ASSEMBLIES_DLL}
  )

  set(TESTS_SRC
    test/ClockTest.cs
    test/CreateNodeTest.cs
    test/InitShutdownTest.cs
    test/LargeMessageTest.cs
    test/MessagesTest.cs
    test/NodeTest.cs
    test/NativeMetodsTest.cs
    test/SubscriptionTest.cs
    test/TestUtils.cs
  )

  add_dotnet_test(ros2cs_tests
    ${TESTS_SRC}
    INCLUDE_DLLS
    ${ASSEMBLIES_DEP_DLLS}
  )
endif()

ament_package()