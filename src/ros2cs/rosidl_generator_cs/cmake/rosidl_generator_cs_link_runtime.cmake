# Link generated C bindings using the targets exported by Lyrical ROSIDL.
# Use the plain signature, matching the other links on these generated targets.
function(rosidl_generator_cs_link_runtime target)
  target_link_libraries(${target}
    rosidl_runtime_c::rosidl_runtime_c
    rosidl_typesupport_c::rosidl_typesupport_c
    rosidl_typesupport_interface::rosidl_typesupport_interface)
  foreach(package IN LISTS ARGN)
    set(runtime_target "${package}::${package}__rosidl_generator_c")
    if(NOT TARGET "${runtime_target}")
      message(FATAL_ERROR "Missing generated C runtime target: ${runtime_target}")
    endif()
    target_link_libraries(${target} "${runtime_target}")
  endforeach()
endfunction()
