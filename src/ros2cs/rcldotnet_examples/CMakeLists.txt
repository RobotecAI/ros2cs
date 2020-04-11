cmake_minimum_required(VERSION 3.5)

project(rcldotnet_examples C)

find_package(ament_cmake REQUIRED)
find_package(rcldotnet REQUIRED)
find_package(std_msgs REQUIRED)
find_package(geometry_msgs REQUIRED)
find_package(sensor_msgs REQUIRED)
find_package(tf2_msgs REQUIRED)
find_package(nav_msgs REQUIRED)
find_package(dotnet_cmake_module REQUIRED)
find_package(rosidl_generator_cs REQUIRED)

set(CSHARP_TARGET_FRAMEWORK "netcoreapp2.0")
find_package(DotNETExtra REQUIRED)

set(_assemblies_dep_dlls
    ${rcldotnet_ASSEMBLIES_DLL}
    ${std_msgs_ASSEMBLIES_DLL}
    ${geometry_msgs_ASSEMBLIES_DLL}
    ${nav_msgs_ASSEMBLIES_DLL}
    ${tf2_msgs_ASSEMBLIES_DLL}
    ${sensor_msgs_ASSEMBLIES_DLL}
)

# message("Included assemblies for examples: ${_assemblies_dep_dlls}")

add_dotnet_executable(rcldotnet_talker
  RCLDotnetTalker.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)
add_dotnet_executable(rcldotnet_listener
  RCLDotnetListener.cs
  INCLUDE_DLLS
  ${_assemblies_dep_dlls}
)

install_dotnet(rcldotnet_talker DESTINATION lib/${PROJECT_NAME}/dotnet)
install_dotnet(rcldotnet_listener DESTINATION lib/${PROJECT_NAME}/dotnet)

ament_package()
