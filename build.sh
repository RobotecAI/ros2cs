#!/bin/bash

TESTS=0
MSG="Build started."
if [ "$1" = "--with-tests" ]; then
    TESTS=1
    MSG="$MSG (with tests)"
else [ "$1" = "-h" ] || [ "$1" = "--help" ]
    echo "Usage: "
    echo "deploy_unity_plugins.ps1 [--with-tests]"
    echo ""
    echo "Options:"
    echo "--with-tests - build with tests."
    exit 1
fi

echo $MSG
colcon build --merge-install --event-handlers console_direct+ --cmake-args -DCMAKE_BUILD_TYPE=Release -DBUILD_TESTING=$TESTS
