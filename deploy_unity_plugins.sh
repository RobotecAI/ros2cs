#!/bin/bash

SCRIPT=$(readlink -f $0)
SCRIPTPATH=`dirname $SCRIPT`

if [ $# -eq 0 ]; then
  echo "Required argument: target plugins directory"
  exit 1
fi

pluginDir=$1

cp --verbose $SCRIPTPATH/install/lib/dotnet/* ${pluginDir}
mkdir -p  ${pluginDir}/Linux/x86_64/
cp --verbose $SCRIPTPATH/install/standalone/* ${pluginDir}/Linux/x86_64/
cp --verbose $SCRIPTPATH/install/lib/*.so ${pluginDir}/Linux/x86_64/
