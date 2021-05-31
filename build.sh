#!/bin/bash
colcon build --merge-install --event-handlers console_cohesion+ --cmake-args -DCMAKE_BUILD_TYPE=Release
