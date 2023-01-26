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

find_package(rosidl_generator_c REQUIRED)
find_package(rosidl_typesupport_c REQUIRED)
find_package(rosidl_typesupport_interface REQUIRED)
find_package(ament_cmake_export_assemblies REQUIRED)
find_package(dotnet_cmake_module REQUIRED)
find_package(DotNETExtra REQUIRED) # add_dotnet_library
find_package(ros2cs_common REQUIRED)

# Get a list of typesupport implementations from valid rmw implementations.
rosidl_generator_cs_get_typesupports(_typesupport_impls)

if(_typesupport_impls STREQUAL "")
  message(WARNING "No valid typesupport for .NET generator. .NET messages will not be generated.")
  return()
endif()

set(_output_path "${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_cs/${PROJECT_NAME}")
set(_generated_msg_cs_files "")
set(_generated_msg_c_files "")
set(_generated_msg_c_ts_files "")
set(_generated_srv_cs_files "")
set(_generated_srv_c_files "")
set(_generated_srv_c_ts_files "")

if(NOT WIN32)
  if(CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")
  elseif(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,-undefined,error")
  endif()
endif()

# For each IDL file
foreach(_idl_file ${rosidl_generate_interfaces_ABS_IDL_FILES})
  get_filename_component(_parent_folder "${_idl_file}" DIRECTORY)
  get_filename_component(_parent_folder "${_parent_folder}" NAME)
  get_filename_component(_msg_name "${_idl_file}" NAME_WE)
  get_filename_component(_ext "${_idl_file}" EXT)
  string_camel_case_to_lower_case_underscore("${_msg_name}" _module_name)

  if(_parent_folder STREQUAL "msg")
    list(APPEND _generated_msg_cs_files
      "${_output_path}/${_parent_folder}/${_module_name}.cs"
    )
    list(APPEND _generated_msg_c_files
      "${_output_path}/${_parent_folder}/${_module_name}_s.c"
    )
    foreach(_typesupport_impl ${_typesupport_impls})
        list_append_unique(_generated_msg_c_ts_files
          "${_output_path}/${_parent_folder}/${_module_name}.ep.${_typesupport_impl}.c"
        )
        list(APPEND _type_support_by_generated_msg_c_files "${_typesupport_impl}")
    endforeach()
  elseif(_parent_folder STREQUAL "srv")
    list(APPEND _generated_srv_cs_files
      "${_output_path}/${_parent_folder}/${_module_name}.cs"
    )
    list(APPEND _generated_srv_c_files
      "${_output_path}/${_parent_folder}/${_module_name}_s.c"
    )
    foreach(_typesupport_impl ${_typesupport_impls})
        list_append_unique(_generated_srv_c_ts_files
          "${_output_path}/${_parent_folder}/${_module_name}.ep.${_typesupport_impl}.c"
        )
        list(APPEND _type_support_by_generated_srv_c_files "${_typesupport_impl}")
    endforeach()
  elseif(_parent_folder STREQUAL "action")
  else()
    message(FATAL_ERROR "Interface file with unknown parent folder: ${_idl_file}")
  endif()
endforeach()

if( (_generated_msg_c_files STREQUAL "") AND (_generated_srv_c_files STREQUAL "") )
  return()
endif()

set(_dependency_files "")
set(_dependencies "")
foreach(_pkg_name ${rosidl_generate_interfaces_DEPENDENCY_PACKAGE_NAMES})
  foreach(_idl_file ${${_pkg_name}_INTERFACE_FILES})
    set(_abs_idl_file "${${_pkg_name}_DIR}/../${_idl_file}")
    normalize_path(_abs_idl_file "${_abs_idl_file}")
    list(APPEND _dependency_files "${_abs_idl_file}")
    list(APPEND _dependencies "${_pkg_name}:${_abs_idl_file}")
  endforeach()
endforeach()

set(target_dependencies
  "${rosidl_generator_cs_BIN}"
  ${rosidl_generator_cs_GENERATOR_FILES}
  "${rosidl_generator_cs_TEMPLATE_DIR}/idl.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/idl_typesupport.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/idl.cs.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/msg.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/msg_typesupport.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/msg.cs.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/srv.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/srv_typesupport.c.em"
  "${rosidl_generator_cs_TEMPLATE_DIR}/srv.cs.em"
  ${rosidl_generate_interfaces_ABS_IDL_FILES}
  ${_dependency_files}
)

foreach(dep ${target_dependencies})
  if(NOT EXISTS "${dep}")
    message(FATAL_ERROR "Target dependency '${dep}' does not exist")
  endif()
endforeach()

# ROSIDL generator arguments
set(generator_arguments_file "${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_cs__arguments.json")
rosidl_write_generator_arguments(
  "${generator_arguments_file}"
  PACKAGE_NAME "${PROJECT_NAME}"
  IDL_TUPLES "${rosidl_generate_interfaces_IDL_TUPLES}"
  ROS_INTERFACE_DEPENDENCIES "${_dependencies}"
  OUTPUT_DIR "${_output_path}"
  TEMPLATE_DIR "${rosidl_generator_cs_TEMPLATE_DIR}"
  TARGET_DEPENDENCIES ${target_dependencies}
)

file(MAKE_DIRECTORY "${_output_path}")

message(STATUS "Generating C# code for ROS interfaces ${_generated_msg_cs_files} and ${_generated_srv_cs_files}")
set(ros2_distro "$ENV{ROS_DISTRO}")
if(ros2_distro STREQUAL "foxy" OR ros2_distro STREQUAL "galactic")
  set(PYTHON_CMD ${PYTHON_EXECUTABLE})
else()
  set(PYTHON_CMD Python3::Interpreter)
endif()

add_custom_command(
  OUTPUT ${_generated_msg_cs_files} ${_generated_msg_c_files} ${_generated_msg_c_ts_files} ${_generated_srv_cs_files} ${_generated_srv_c_files} ${_generated_srv_c_ts_files}
  COMMAND ${PYTHON_CMD}
  ARGS ${rosidl_generator_cs_BIN}
  --generator-arguments-file "${generator_arguments_file}"
  --typesupport-impls "${_typesupport_impls}"
  --cs-build-tool "${CSBUILD_TOOL}"
  DEPENDS ${target_dependencies}
  COMMENT "Generating C# code for ROS interfaces"
  VERBATIM
)

message(STATUS "Adding custom target")

set(_target_suffix "__cs")
if(TARGET ${rosidl_generate_interfaces_TARGET}${_target_suffix})
  message(WARNING "Custom target ${rosidl_generate_interfaces_TARGET}${_target_suffix} already exists")
else()
  add_custom_target(
    ${rosidl_generate_interfaces_TARGET}${_target_suffix}
    DEPENDS
    ${_generated_msg_cs_files}
    ${_generated_msg_c_ts_files}
    ${_generated_msg_c_files}
    ${_generated_srv_cs_files}
    ${_generated_srv_c_ts_files}
    ${_generated_srv_c_files}
  )
endif()

set_property(
  SOURCE
  ${_generated_msg_cs_files} ${_generated_msg_c_files} ${_generated_msg_c_ts_files} ${_generated_srv_cs_files} ${_generated_srv_c_files} ${_generated_srv_c_ts_files}
  PROPERTY GENERATED 1)

foreach(_generated_msg_c_ts_file ${_generated_msg_c_ts_files})
  get_filename_component(_full_folder "${_generated_msg_c_ts_file}" DIRECTORY)
  get_filename_component(_package_folder "${_full_folder}" DIRECTORY)
  get_filename_component(_package_name "${_package_folder}" NAME)
  get_filename_component(_parent_folder "${_full_folder}" NAME)
  get_filename_component(_base_msg_name "${_generated_msg_c_ts_file}" NAME_WE)
  get_filename_component(_full_extension_msg_name "${_generated_msg_c_ts_file}" EXT)

  set(_msg_name "${_base_msg_name}${_full_extension_msg_name}")

  list(FIND _generated_msg_c_ts_files ${_generated_msg_c_ts_file} _file_index)
  list(GET _type_support_by_generated_msg_c_files ${_file_index} _typesupport_impl)

  find_package(${_typesupport_impl} REQUIRED)

  set(_target_name "${_package_name}_${_base_msg_name}__${_typesupport_impl}")

  string_camel_case_to_lower_case_underscore("${_module_name}" _header_name)

  add_library(${_target_name} SHARED
    "${_generated_msg_c_ts_file}"
    "${_generated_msg_c_files}"
  )

  set(_destination_dir "${_output_path}/${_parent_folder}")
  set(_extension_compile_flags "")
  if(NOT WIN32)
    set(_extension_compile_flags "-Wall -Wextra")
  endif()

  set_target_properties(
    ${_target_name}
    PROPERTIES
    COMPILE_FLAGS                           "${_extension_compile_flags}"
    OUTPUT_NAME                             "${_target_name}_native"
    RUNTIME_OUTPUT_DIRECTORY                ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_DEBUG          ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_RELEASE        ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL     ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY                ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_DEBUG          ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_RELEASE        ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL     ${_destination_dir}
  )

  list(APPEND _extension_dependencies ${_target_name})

  set(_extension_link_flags "")
  if(NOT WIN32)
    if(CMAKE_COMPILER_IS_GNUCXX)
      set(_extension_link_flags "-Wl,--no-undefined")
    elseif(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
      set(_extension_link_flags "-Wl,-undefined,error")
    endif()
  endif()

  message("Link libraries: ${PROJECT_NAME}__${_typesupport_impl}")
  target_link_libraries(${_target_name}
    ${PROJECT_NAME}__${_typesupport_impl}
    ${_extension_link_flags}
    ${PROJECT_NAME}__rosidl_generator_c
  )

  set(ros2_distro "$ENV{ROS_DISTRO}")

  if(ros2_distro STREQUAL "humble" OR ros2_distro STREQUAL "rolling")
    rosidl_get_typesupport_target(c_typesupport_target "${PROJECT_NAME}" "rosidl_typesupport_c")
    target_link_libraries(${_target_name} "${c_typesupport_target}")
  else()
    rosidl_target_interfaces(${_target_name} ${PROJECT_NAME} rosidl_typesupport_c)
  endif()

  target_include_directories(${_target_name}
    PUBLIC
    ${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_c
    ${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_cs
  )

  ament_target_dependencies(${_target_name}
    "rosidl_generator_c"
    "rosidl_generator_cs"
    "rosidl_typesupport_c"
    "rosidl_typesupport_interface"
  )

  foreach(_pkg_name ${rosidl_generate_interfaces_DEPENDENCY_PACKAGE_NAMES})
    ament_target_dependencies(${_target_name}
      ${_pkg_name}
    )
  endforeach()

  add_dependencies(${_target_name}
    ${rosidl_generate_interfaces_TARGET}__${_typesupport_impl}
  )

  if(NOT rosidl_generate_interfaces_SKIP_INSTALL)
    install(TARGETS ${_target_name} EXPORT ${_target_name}
      ARCHIVE DESTINATION lib
      LIBRARY DESTINATION lib
      RUNTIME DESTINATION bin
    )
  endif()

endforeach()

foreach(_generated_srv_c_ts_file ${_generated_srv_c_ts_files})
  get_filename_component(_full_folder "${_generated_srv_c_ts_file}" DIRECTORY)
  get_filename_component(_package_folder "${_full_folder}" DIRECTORY)
  get_filename_component(_package_name "${_package_folder}" NAME)
  get_filename_component(_parent_folder "${_full_folder}" NAME)
  get_filename_component(_base_srv_name "${_generated_srv_c_ts_file}" NAME_WE)
  get_filename_component(_full_extension_srv_name "${_generated_srv_c_ts_file}" EXT)

  set(_srv_name "${_base_srv_name}${_full_extension_srv_name}")

  list(FIND _generated_srv_c_ts_files ${_generated_srv_c_ts_file} _file_index)
  list(GET _type_support_by_generated_srv_c_files ${_file_index} _typesupport_impl)

  find_package(${_typesupport_impl} REQUIRED)

  set(_target_name "${_package_name}_srv_${_base_srv_name}__${_typesupport_impl}")

  string_camel_case_to_lower_case_underscore("${_module_name}" _header_name)

  add_library(${_target_name} SHARED
    "${_generated_srv_c_ts_file}"
    "${_generated_srv_c_files}"
  )

  set(_destination_dir "${_output_path}/${_parent_folder}")
  set(_extension_compile_flags "")
  if(NOT WIN32)
    set(_extension_compile_flags "-Wall -Wextra")
  endif()

  set_target_properties(
    ${_target_name}
    PROPERTIES
    COMPILE_FLAGS                           "${_extension_compile_flags}"
    OUTPUT_NAME                             "${_target_name}_native"
    RUNTIME_OUTPUT_DIRECTORY                ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_DEBUG          ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_RELEASE        ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO ${_destination_dir}
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL     ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY                ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_DEBUG          ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_RELEASE        ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_RELWITHDEBINFO ${_destination_dir}
    LIBRARY_OUTPUT_DIRECTORY_MINSIZEREL     ${_destination_dir}
  )

  list(APPEND _extension_dependencies ${_target_name})

  set(_extension_link_flags "")
  if(NOT WIN32)
    if(CMAKE_COMPILER_IS_GNUCXX)
      set(_extension_link_flags "-Wl,--no-undefined")
    elseif(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
      set(_extension_link_flags "-Wl,-undefined,error")
    endif()
  endif()

  message("Link libraries: ${PROJECT_NAME}__${_typesupport_impl}")
  target_link_libraries(${_target_name}
    ${PROJECT_NAME}__${_typesupport_impl}
    ${_extension_link_flags}
    ${PROJECT_NAME}__rosidl_generator_c
  )

  set(ros2_distro "$ENV{ROS_DISTRO}")

  if(ros2_distro STREQUAL "humble" OR ros2_distro STREQUAL "rolling")
    rosidl_get_typesupport_target(c_typesupport_target "${PROJECT_NAME}" "rosidl_typesupport_c")
    target_link_libraries(${_target_name} "${c_typesupport_target}")
  else()
    rosidl_target_interfaces(${_target_name} ${PROJECT_NAME} rosidl_typesupport_c)
  endif()

  target_include_directories(${_target_name}
    PUBLIC
    ${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_c
    ${CMAKE_CURRENT_BINARY_DIR}/rosidl_generator_cs
  )

  ament_target_dependencies(${_target_name}
    "rosidl_generator_c"
    "rosidl_generator_cs"
    "rosidl_typesupport_c"
    "rosidl_typesupport_interface"
  )

  foreach(_pkg_name ${rosidl_generate_interfaces_DEPENDENCY_PACKAGE_NAMES})
    ament_target_dependencies(${_target_name}
      ${_pkg_name}
    )
  endforeach()

  add_dependencies(${_target_name}
    ${rosidl_generate_interfaces_TARGET}__${_typesupport_impl}
  )

  if(NOT rosidl_generate_interfaces_SKIP_INSTALL)
    install(TARGETS ${_target_name} EXPORT ${_target_name}
      ARCHIVE DESTINATION lib
      LIBRARY DESTINATION lib
      RUNTIME DESTINATION bin
    )
  endif()

endforeach()

message("Install targets")
if(NOT rosidl_generate_interfaces_SKIP_INSTALL)
  install(TARGETS ${_target_name_lib} EXPORT ${_target_name}
    ARCHIVE DESTINATION lib
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin)
endif()

set(_assembly_deps_dll "")

foreach(_assembly_dep ${ros2cs_common_ASSEMBLIES_DLL})
  list(APPEND _assembly_deps_dll "${_assembly_dep}")
endforeach()

foreach(_pkg_name ${rosidl_generate_interfaces_DEPENDENCY_PACKAGE_NAMES})
  find_package(${_pkg_name} REQUIRED)
  foreach(_assembly_dep ${${_pkg_name}_ASSEMBLIES_DLL})
    list(APPEND _assembly_deps_dll "${_assembly_dep}")
  endforeach()
endforeach()

add_dotnet_library(${PROJECT_NAME}_assembly
  SOURCES
  ${_generated_msg_cs_files} ${_generated_srv_cs_files}
  INCLUDE_DLLS
  ${_assembly_deps_dll}
)

add_dependencies(${PROJECT_NAME}_assembly
  "${rosidl_generate_interfaces_TARGET}${_target_suffix}"
)

if(NOT rosidl_generate_interfaces_SKIP_INSTALL)
  set(_install_assembly_dir "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}")
  if(NOT _generated_msg_cs_files STREQUAL "")
    list(GET _generated_msg_cs_files 0 _msg_file)
    get_filename_component(_msg_package_dir "${_msg_file}" DIRECTORY)
    get_filename_component(_msg_package_dir "${_msg_package_dir}" DIRECTORY)
    install_dotnet(${PROJECT_NAME}_assembly DESTINATION "lib/dotnet")
    ament_export_assemblies_dll("lib/dotnet/${PROJECT_NAME}_assembly.dll")
  endif()

  if(NOT _generated_srv_cs_files STREQUAL "")
    list(GET _generated_srv_cs_files 0 _msg_file)
    get_filename_component(_msg_package_dir "${_msg_file}" DIRECTORY)
    get_filename_component(_msg_package_dir "${_msg_package_dir}" DIRECTORY)
    install_dotnet(${PROJECT_NAME}_assembly DESTINATION "lib/dotnet")
    ament_export_assemblies_dll("lib/dotnet/${PROJECT_NAME}_assembly.dll")
  endif()
endif()
