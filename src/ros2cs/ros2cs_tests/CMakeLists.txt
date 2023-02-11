cmake_minimum_required(VERSION 3.6)
project(ros2cs_tests)

if(BUILD_TESTING)
  find_package(ros2cs_core REQUIRED)
  find_package(ament_cmake REQUIRED)
  find_package(dotnet_cmake_module REQUIRED)

  set(CSHARP_TARGET_FRAMEWORK "netcoreapp6.0")

  set(CSBUILD_TOOL "DotNetCore")
  find_package(DotNETExtra REQUIRED)

  find_package(std_msgs REQUIRED)
  find_package(test_msgs REQUIRED)
  find_package(example_interfaces REQUIRED)

  set(DOTNET_OUTPUT_PATH
    ${CSHARP_BUILDER_OUTPUT_PATH}/${CSHARP_TARGET_FRAMEWORK}/
  )

  set(ASSEMBLIES_DEP_DLLS
      ${ros2cs_core_ASSEMBLIES_DLL}
      ${ros2cs_common_ASSEMBLIES_DLL}
      ${std_msgs_ASSEMBLIES_DLL}
      ${test_msgs_ASSEMBLIES_DLL}
      ${example_interfaces_ASSEMBLIES_DLL}
  )

  set(TESTS_SRC
    src/ClockTest.cs
    src/CreateNodeTest.cs
    src/InitShutdownTest.cs
    src/LargeMessageTest.cs
    src/MessagesTest.cs
    src/NodeTest.cs
    src/NativeMetodsTest.cs
    src/ClientTest.cs
    src/ServiceTest.cs
    src/SubscriptionTest.cs
    src/TestUtils.cs
  )

  add_dotnet_test(ros2cs_tests
    ${TESTS_SRC}
    INCLUDE_DLLS
    ${ASSEMBLIES_DEP_DLLS}
  )

  install_dotnet(${PROJECT_NAME} DESTINATION lib/tests)

  ament_package()
endif()