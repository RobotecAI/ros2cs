cmake_minimum_required(VERSION 3.8)

project(hello C)

# find dependencies
find_package(ament_cmake REQUIRED)
find_package(ros2cs_common REQUIRED)
find_package(ros2cs_core REQUIRED)
find_package(hello_interfaces REQUIRED)
find_package(example_interfaces REQUIRED)
find_package(builtin_interfaces REQUIRED)
find_package(dotnet_cmake_module REQUIRED)
find_package(rosidl_generator_cs REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
find_package(DotNETExtra REQUIRED)

set(_assemblies_dep_dlls
    ${ros2cs_common_ASSEMBLIES_DLL}
    ${ros2cs_core_ASSEMBLIES_DLL}
    ${hello_interfaces_ASSEMBLIES_DLL}
    ${example_interfaces_ASSEMBLIES_DLL}
    ${builtin_interfaces_ASSEMBLIES_DLL}
)

add_dotnet_executable(hello_client
  HelloClient.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

add_dotnet_executable(hello_service
  HelloService.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

install_dotnet(hello_client DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(hello_service DESTINATION lib/${PROJECT_NAME}/dotnet)

ament_package()
