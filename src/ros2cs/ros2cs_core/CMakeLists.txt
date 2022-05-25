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
find_package(dotnet_cmake_module REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
find_package(DotNETExtra REQUIRED)

# Used by ros2cs_native
find_package(rcl REQUIRED)
find_package(rcutils REQUIRED)
find_package(rmw REQUIRED)
find_package(rosidl_generator_c REQUIRED)

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
  "rcl"
  "rcutils"
  "rmw"
  "rosidl_generator_c"
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
ament_export_dependencies(rcl)
ament_export_dependencies(rosidl_generator_c)

option(STANDALONE_BUILD "Deploy standalone libraries with build" ON)

set(ros2_distro "$ENV{ROS_DISTRO}")

# There is an issue with resolving environment variable paths in runtime for "rcpputils" library on ros2 foxy.
# We replacing this library with our patched version. See "resources/rcpputils.diff".
if(ros2_distro STREQUAL "foxy")
  if(WIN32)
    install(FILES "${CMAKE_CURRENT_LIST_DIR}/../../../resources/foxy/Windows/x86_64/rcpputils.dll"
    DESTINATION resources
    )
  elseif(UNIX)
    install(FILES "${CMAKE_CURRENT_LIST_DIR}/../../../resources/foxy/Linux/x86_64/librcpputils.so"
      DESTINATION resources
    )
  endif()
endif()

if(STANDALONE_BUILD)
  find_program(PATCHELF "patchelf")
  if(UNIX)
    if(NOT PATCHELF)
      message(FATAL_ERROR "'patchelf' executable not found. Linux standalone version requires 'patchelf' for patching 'runpath' of certain libraries. You can install it via 'sudo apt install patchelf'")
    else()
      message("'patchelf' found in ${PATCHELF}")
    endif()
  endif()
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
      if(WIN32)
        get_target_property(DDSC_LIB_PATH CycloneDDS::ddsc IMPORTED_IMPLIB_RELWITHDEBINFO)
      elseif(UNIX)
        get_target_property(DDSC_LIB_PATH CycloneDDS::ddsc IMPORTED_LOCATION_NONE)
        if(DDSC_LIB_PATH STREQUAL "DDSC_LIB_PATH-NOTFOUND")
          get_target_property(DDSC_LIB_PATH CycloneDDS::ddsc IMPORTED_LOCATION_RELWITHDEBINFO)
        endif()
      endif()
      list(APPEND REQ_STANDALONE_LIBS ${DDSC_LIB_PATH})
    endif()

    # Get rmw_cyclonedds_cpp for humble
    if("${_library_name}" STREQUAL "rmw_cyclonedds_cpp" AND UNIX AND (ros2_distro STREQUAL "humble" OR ros2_distro STREQUAL "rolling"))
      get_target_property(RMW_CYCLONE_CPP rmw_cyclonedds_cpp::rmw_cyclonedds_cpp IMPORTED_LOCATION_NOCONFIG)
      list(APPEND REQ_STANDALONE_LIBS ${RMW_CYCLONE_CPP})
    endif()

    # Get cyclonedds iceoryx dependency for ros2 galaxy (unix only)
    if(UNIX AND NOT ros2_distro STREQUAL "foxy")
      if("${_library_name}" STREQUAL "iceoryx_binding_c" )
        get_target_property(ICEORYX_LIB_PATH iceoryx_binding_c::iceoryx_binding_c IMPORTED_LOCATION_NONE)
        if(ICEORYX_LIB_PATH STREQUAL "ICEORYX_LIB_PATH-NOTFOUND")
          get_target_property(ICEORYX_LIB_PATH iceoryx_binding_c::iceoryx_binding_c IMPORTED_LOCATION_NOCONFIG)
        endif()
        list(APPEND REQ_STANDALONE_LIBS ${ICEORYX_LIB_PATH})
      elseif("${_library_name}" STREQUAL "iceoryx_posh")
        get_target_property(ICEORYX_POSH_LIB_PATH iceoryx_posh::iceoryx_posh IMPORTED_LOCATION_NONE)
        if(ICEORYX_POSH_LIB_PATH STREQUAL "ICEORYX_POSH_LIB_PATH-NOTFOUND")
          get_target_property(ICEORYX_POSH_LIB_PATH iceoryx_posh::iceoryx_posh IMPORTED_LOCATION_NOCONFIG)
        endif()
        list(APPEND REQ_STANDALONE_LIBS ${ICEORYX_POSH_LIB_PATH})
      elseif("${_library_name}" STREQUAL "iceoryx_utils")
        get_target_property(ICEORYX_UTILS_LIB_PATH iceoryx_utils::iceoryx_utils IMPORTED_LOCATION_NONE)
        get_target_property(ICEORYX_PLATFORM_LIB_PATH iceoryx_utils::iceoryx_platform IMPORTED_LOCATION_NONE)
        list(APPEND REQ_STANDALONE_LIBS ${ICEORYX_UTILS_LIB_PATH} ${ICEORYX_PLATFORM_LIB_PATH})
      elseif("${_library_name}" STREQUAL "iceoryx_hoofs")
        get_target_property(ICEORYX_HOOFS_LIB_PATH iceoryx_hoofs::iceoryx_hoofs IMPORTED_LOCATION_NOCONFIG)
        get_target_property(ICEORYX_PLATFORM_LIB_PATH iceoryx_hoofs::iceoryx_platform IMPORTED_LOCATION_NOCONFIG)
        list(APPEND REQ_STANDALONE_LIBS ${ICEORYX_HOOFS_LIB_PATH} ${ICEORYX_PLATFORM_LIB_PATH})
      endif()
    endif()

    # We skip python static libs on windows
    if(WIN32)
      foreach(entry ${${_library_name}_LIBRARIES})
        string(REGEX MATCH ".*libs\/python[0-9]*\.lib" _found ${entry})
        if(_found STREQUAL "")
          list(APPEND REQ_STANDALONE_LIBS ${entry})
        endif()
      endforeach()
    else()
      list(APPEND REQ_STANDALONE_LIBS ${${_library_name}_LIBRARIES})
    endif()
  endmacro()

  macro(install_standalone_dependencies)
    # Filter valid libraries
    list(FILTER REQ_STANDALONE_LIBS INCLUDE REGEX ".*(lib|dll|so)(\.[0-9])*$")
    list(REMOVE_DUPLICATES REQ_STANDALONE_LIBS)

    if(WIN32)
      foreach(lib_path ${REQ_STANDALONE_LIBS})
        string(REGEX REPLACE "\/(l|L)ib\/" "/bin/" bin_path ${lib_path})
        string(REGEX REPLACE "\.lib$" ".dll" dll_path ${bin_path})
        list(APPEND REQ_STANDALONE_DLLS ${dll_path})
      endforeach()
      install(FILES ${REQ_STANDALONE_DLLS}
        DESTINATION standalone
      )
    elseif(UNIX)
      set(_resolvedFiles "")
      foreach(lib_path ${REQ_STANDALONE_LIBS})
        # Generate soversion files (.so.<major>). Some libs links to soversion symlinks which are not returned by find_package by default.
        get_filename_component(_resolvedFile "${lib_path}" REALPATH)
        list(APPEND _resolvedFiles "${_resolvedFile}")
      endforeach()

      install(FILES ${_resolvedFiles}
        DESTINATION standalone
      )

      # Fix soversion files
      foreach(_resolvedFile ${_resolvedFiles})
        if("${_resolvedFile}" MATCHES "so(\.[0-9]*)+$")
          # message("Soversion file detected ${_resolvedFile}")

          # Get file path without so
          string(FIND "${_resolvedFile}" ".so." _findPos)
          string(SUBSTRING "${_resolvedFile}" 0 ${_findPos} _cutted)

          set(_remainingPath "${_resolvedFile}")
          while("${_remainingPath}" MATCHES "so(\.[0-9]*)+$")
            string(FIND "${_remainingPath}" "." _lastDotPos REVERSE)
            string(SUBSTRING "${_remainingPath}" 0 ${_lastDotPos} _tempPos)
            get_filename_component(_libPathFilename "${_tempPos}" NAME)
            get_filename_component(_resolvedFilename "${_resolvedFile}" NAME)
            # message("ln -s ${_resolvedFilename} ${_libPathFilename}")
            install(CODE "execute_process(COMMAND ln -s ${_resolvedFilename} ${_libPathFilename} WORKING_DIRECTORY ${CMAKE_INSTALL_PREFIX}/standalone ERROR_QUIET)")
            set(_remainingPath "${_tempPos}")
          endwhile()
        endif()
      endforeach()

      # rpath for each standalone lib must be updated so all the libs will see each other
      install(CODE 
        "execute_process(COMMAND find ${CMAKE_INSTALL_PREFIX}/standalone -name *.so -exec patchelf --set-rpath $ORIGIN:. {} \;)"
      )
      # soversion files as well
      install(CODE 
        "execute_process(COMMAND find ${CMAKE_INSTALL_PREFIX}/standalone -name *.so.* -exec patchelf --set-rpath $ORIGIN:. {} \;)"
      )
      # as well as custom resources
      install(CODE 
        "execute_process(COMMAND find ${CMAKE_INSTALL_PREFIX}/resources -name *.so -exec patchelf --set-rpath $ORIGIN:. {} \;)"
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
    FastRTPS
    rmw_fastrtps_cpp
    CycloneDDS
    rmw_cyclonedds_cpp
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
  
  if(UNIX AND NOT ros2_distro STREQUAL "foxy")
    if(ros2_distro STREQUAL "galactic")
      list(APPEND ros2_standalone_libs iceoryx_binding_c iceoryx_posh iceoryx_utils)
    else()
      list(APPEND ros2_standalone_libs iceoryx_binding_c iceoryx_posh iceoryx_hoofs)
    endif()
  endif()

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
  endif()
endif()

ament_package()
