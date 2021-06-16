# Original Copyright:
# Copyright (C) 2015-2017, Illumina, inc.
#
# Based on
# https://github.com/Illumina/interop/tree/master/cmake/Modules
#
# Find the msbuild tool
#
# DotNetCore_FOUND             System has msbuild
# DotNetCore_EXECUTABLE        Where to find csc
# DotNetCore_VERSION           The version number of the DotNet framework

set(DotNetCore_ROOT "" CACHE PATH "Set the location of the .NET root directory")
set(DotNetCore_VERSION "" CACHE STRING "C# .NET compiler version")

if(DotNetCore_ROOT AND EXISTS "${DotNetCore_ROOT}")
    find_program(DotNetCore_EXECUTABLE dotnet dotnet.exe
            PATHS ${DotNetCore_ROOT}
            PATH_SUFFIXES . bin
            NO_DEFAULT_PATH)
endif()


find_program(DotNetCore_EXECUTABLE dotnet dotnet.exe)

if(EXISTS "${DotNetCore_EXECUTABLE}")
    execute_process(
        COMMAND ${DotNetCore_EXECUTABLE} --version
        OUTPUT_VARIABLE dotnet_core_version_string
        OUTPUT_STRIP_TRAILING_WHITESPACE
   )
    string(REGEX MATCH "([0-9]*)([.])([0-9]*)([.]*)([0-9]*)" dotnet_core_version_string "${dotnet_core_version_string}")
    set(DotNetCore_VERSION ${dotnet_core_version_string} CACHE STRING ".NET coreclr version" FORCE)
endif()

include(FindPackageHandleStandardArgs)
find_package_handle_standard_args(DotNetCore DEFAULT_MSG DotNetCore_EXECUTABLE)
mark_as_advanced(DotNetCore_EXECUTABLE DotNetCore_VERSION)

