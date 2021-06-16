#!/bin/bash
colcon build --merge-install --event-handlers console_direct+ --executor sequential
