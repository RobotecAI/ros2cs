#!/bin/bash
#NOTE: modify the directory to where your Unity project is
pluginDir=/home/adam/robotec/unity-demo-scene/src/unity-demo-scene/unity-demo-scene/Assets/Plugins/
cp --verbose install/lib/dotnet/* ${pluginDir}
cp --verbose install/lib/*.so ${pluginDir}/x86_64/
