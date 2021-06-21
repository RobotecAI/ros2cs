#!/bin/bash
#NOTE: modify the directory to where your Unity project is

if [ $# -eq 0 ]; then
  echo "Required argument: target plugins directory"
  exit 1
fi

pluginDir=$1

cp --verbose install/lib/dotnet/* ${pluginDir}
cp --verbose install/lib/*.so ${pluginDir}/Linux/x86_64/
