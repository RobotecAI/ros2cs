cmake_minimum_required(VERSION 3.6)
project(ros2cs_tests)

if(BUILD_TESTING)
  find_package(ros2cs_core REQUIRED)
  find_package(ament_cmake REQUIRED)
  find_package(dotnet_cmake_module REQUIRED)

  if(UNIX)
    find_program(LSB_RELEASE_EXEC lsb_release)
    execute_process(COMMAND ${LSB_RELEASE_EXEC} -rs
        OUTPUT_VARIABLE LSB_RELEASE_ID_SHORT
        OUTPUT_STRIP_TRAILING_WHITESPACE
    )
    if("${LSB_RELEASE_ID_SHORT}" STREQUAL "22.04")
      set(CSHARP_TARGET_FRAMEWORK "net6.0")
    else()
      set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
    endif()
  else()
    set(CSHARP_TARGET_FRAMEWORK "netcoreapp3.1")
  endif()
  find_package(DotNETExtra REQUIRED)

  find_package(std_msgs REQUIRED)
  find_package(test_msgs REQUIRED)

  set(DOTNET_OUTPUT_PATH
    ${CSHARP_BUILDER_OUTPUT_PATH}/${CSHARP_TARGET_FRAMEWORK}/
  )

  set(ASSEMBLIES_DEP_DLLS
      ${ros2cs_core_ASSEMBLIES_DLL}
      ${ros2cs_common_ASSEMBLIES_DLL}
      ${std_msgs_ASSEMBLIES_DLL}
      ${test_msgs_ASSEMBLIES_DLL}
  )

  set(TESTS_SRC
    src/ClockTest.cs
    src/CreateNodeTest.cs
    src/InitShutdownTest.cs
    src/LargeMessageTest.cs
    src/MessagesTest.cs
    src/NodeTest.cs
    src/NativeMetodsTest.cs
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