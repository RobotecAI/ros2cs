ROS2 for C#
=============

A C# module for ROS2, including C# implementation of rcl APIs, message generation, libraries export for Unity, tests and examples.

## Prerequisites

*  ROS2 foxy installed on the system, along with `ros-foxy-test-msgs package`
*  Ubuntu 20.04
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core - [see here](https://www.microsoft.com/net/learn/get-started)

## Building

You need to source your ROS2 installation (e.g. `source /opt/ros/foxy/setup.bash`) before you proceed, for each new open terminal. You can also include this command in your `~/.bashrc` file.

*  Clone this project,
*  navigate to the top project folder and run the `get_repos.sh` script (it will use vcstool to download required ros2 and custom message - if any - packages),
*  run `build.sh` script (it invokes `colcon_build` with `--merge-install` argument),
*  if you wish to test the build, run attached listener/publisher examples - `ros2cs_talker` and `ros2cs_listener`.

### Standalone version

By default, build also deploys standalone libraries in `install/standalone` directory. You can turn this feature off by setting cmake option `STANDALONE_BUILD` to `OFF`.

To run standalone application you must deploy it with libraries from:

* on linux, both `install/lib` and `install/standalone`,
* on windows, both `install/bin` and `install/standalone`.

You should also modify OS environment variables so your executable will find all the required libraries:

* on linux `LD_LIBRARY_PATH`,
* on windows `PATH` (if executable lies in a different directory than libraries).

Example for linux (make sure `ros2cs_examples` package is built):

```bash
cd install/lib/ros2cs_examples
LD_LIBRARY_PATH=$LD_LIBRARY_PATH:`pwd`/../../standalone:`pwd`/.. ./ros2cs_talker
```
