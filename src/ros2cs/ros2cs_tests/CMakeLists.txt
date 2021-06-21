cmake_minimum_required(VERSION 3.5)

project(ros2cs_tests C)

find_package(ament_cmake REQUIRED)
find_package(ros2cs_core REQUIRED)
find_package(std_msgs REQUIRED)
find_package(test_msgs REQUIRED)
find_package(dotnet_cmake_module REQUIRED)
find_package(rosidl_generator_cs REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
find_package(DotNETExtra REQUIRED)

set(_assemblies_dep_dlls
    ${ros2cs_core_ASSEMBLIES_DLL}
    ${std_msgs_ASSEMBLIES_DLL}
    ${test_msgs_ASSEMBLIES_DLL}
    ${rosidl_generator_ASSEMBLIES_DLL}
)

# message("Included assemblies for examples: ${_assemblies_dep_dlls}")

# TODO(sam): implement add_dotnet_tests (different name?), use this placeholder for now:
add_dotnet_library(ros2cs_tests
  PlaceholderClass.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

install_dotnet(ros2cs_tests DESTINATION lib/${PROJECT_NAME}/dotnet)

ament_package()
