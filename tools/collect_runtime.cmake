# SPDX-License-Identifier: Apache-2.0
cmake_minimum_required(VERSION 3.21)
if(POLICY CMP0207)
  cmake_policy(SET CMP0207 NEW)
endif()
if(NOT DEFINED INPUT_FILE)
  message(FATAL_ERROR "INPUT_FILE is required")
endif()
include("${INPUT_FILE}")
if(NOT SEEDS OR NOT OUTPUT_FILE)
  message(FATAL_ERROR "SEEDS and OUTPUT_FILE are required")
endif()
if(CMAKE_HOST_WIN32)
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_PLATFORM windows+pe)
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_TOOL dumpbin)
  find_program(CMAKE_GET_RUNTIME_DEPENDENCIES_COMMAND dumpbin REQUIRED)
  set(_pre "[Aa][Pp][Ii]-[Mm][Ss]-.*" "[Ee][Xx][Tt]-[Mm][Ss]-.*")
  set(_post ".*[/\\\\][Ss][Yy][Ss][Tt][Ee][Mm]32[/\\\\].*")
else()
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_PLATFORM linux+elf)
  set(CMAKE_GET_RUNTIME_DEPENDENCIES_TOOL objdump)
  find_program(CMAKE_GET_RUNTIME_DEPENDENCIES_COMMAND objdump REQUIRED)
  # Do not ship a foreign loader or glibc in a Unity plugin. Host OS supplies these.
  set(_pre "^ld-linux.*" "^lib(c|m|dl|pthread|rt|resolv|util)\\.so(\\..*)?$")
  set(_post)
endif()
file(GET_RUNTIME_DEPENDENCIES
  LIBRARIES ${SEEDS}
  DIRECTORIES ${SEARCH_DIRS}
  RESOLVED_DEPENDENCIES_VAR _resolved
  UNRESOLVED_DEPENDENCIES_VAR _unresolved
  CONFLICTING_DEPENDENCIES_PREFIX _conflicts
  PRE_EXCLUDE_REGEXES ${_pre}
  POST_EXCLUDE_REGEXES ${_post}
)
if(_unresolved)
  message(FATAL_ERROR "Unresolved runtime dependencies: ${_unresolved}")
endif()
# Return every candidate to the driver. It applies the explicitly ordered
# overlay roots and rejects unrelated conflicting basenames, then scans any
# selected replacements to collect their own dependencies.
foreach(_name IN LISTS _conflicts_FILENAMES)
  list(APPEND _resolved ${_conflicts_${_name}})
endforeach()
list(REMOVE_DUPLICATES _resolved)
list(SORT _resolved)
string(REPLACE ";" "\n" _text "${_resolved}")
file(WRITE "${OUTPUT_FILE}" "${_text}\n")
