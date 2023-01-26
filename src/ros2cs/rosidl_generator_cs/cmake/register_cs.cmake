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

macro(rosidl_generator_cs_extras BIN GENERATOR_FILES TEMPLATE_DIR)
  find_package(ament_cmake_core QUIET REQUIRED)

  ament_register_extension(
    "rosidl_generate_idl_interfaces"
    "rosidl_generator_cs"
    "rosidl_generator_cs_generate_interfaces.cmake"
  )

  set(CSBUILD_TOOL "DotNetCore")

  normalize_path(BIN_NORMALIZED "${BIN}")
  set(rosidl_generator_cs_BIN "${BIN_NORMALIZED}")

  normalize_path(GENERATOR_FILES_NORMALIZED "${GENERATOR_FILES}")
  set(rosidl_generator_cs_GENERATOR_FILES "${GENERATOR_FILES_NORMALIZED}")

  normalize_path(TEMPLATE_DIR_NORMALIZED "${TEMPLATE_DIR}")
  set(rosidl_generator_cs_TEMPLATE_DIR "${TEMPLATE_DIR_NORMALIZED}")
endmacro()
