#!/bin/bash
colcon build --merge-install --event-handlers console_direct+ --packages-select ros2cs_examples
