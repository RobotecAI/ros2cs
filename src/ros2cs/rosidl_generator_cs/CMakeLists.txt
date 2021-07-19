cmake_minimum_required(VERSION 3.6)

project(rosidl_generator_cs)

find_package(ament_cmake REQUIRED)
find_package(rosidl_cmake REQUIRED)
find_package(rmw_implementation_cmake REQUIRED)

ament_export_dependencies(ament_cmake)
ament_export_dependencies(rosidl_cmake)
ament_export_dependencies(rmw_implementation_cmake)

ament_index_register_resource("rosidl_generator_packages")

ament_python_install_package(${PROJECT_NAME})

ament_package(
  CONFIG_EXTRAS
    "cmake/rosidl_generator_cs_get_typesupports.cmake"
    "cmake/register_cs.cmake"
    "rosidl_generator_cs-extras.cmake.in"
)

install(
  PROGRAMS bin/rosidl_generator_cs
  DESTINATION lib/rosidl_generator_cs
)

install(
  DIRECTORY cmake resource
  DESTINATION share/${PROJECT_NAME}
)
