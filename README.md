Ros2cs
=============

A C# .Net library for ROS2, including C# implementation of rcl APIs, message generation, tests and examples.

Ros2cs is an independent part of Ros2 For Unity, which enables high-performance communication between simulation and ros2 robot packages.

### Features

- A set of core abstractions such as Node, Publisher, Subscription, QoS, Clock
- Comes with support for all standard ros2 messages
- Custom messages can be easily generated from unmodified ros2 packages
- A logger that can be hooked to your application callbacks (e. g. in Unity3D)

### Platforms

Supported OSes: 
- Ubuntu 20.04
- Windows 10

Supported ros2 distributions:
- foxy
- galactic
Note that these are both supported on the main branch, but you need to build with either `ros2_foxy.repos` or `ros2_galactic.repos`.

Build libraries are supported in two versions:
- standalone (no ros2 installation required on target machine, e. g. your Unity3D simulation server). All required dependencies are installed and can be used e. g. as a complete set of Unity3D plugins.
- with existing (supported) ros2 installation, as an overlay build - only ros2cs libraries and generated messages are installed.

### Prerequisites

*  ROS2 installed on the system, along with `test-msgs` package (e. g. for Foxy: `ros2-foxy-test-msgs`).
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core - [see here](https://www.microsoft.com/net/learn/get-started)

### Building

You need to source your ROS2 installation (e.g. `source /opt/ros/foxy/setup.bash` on Ubuntu) before you proceed, for each new open terminal. You can also include this command in your `~/.bashrc` file.

*  Clone this project,
*  navigate to the top project folder and run the `get_repos.sh` script (it will use `vcstool` to download required ros2 and custom message - if any - packages). By default, this will get Foxy repositories. Note that on Windows the command is a bit different: `vcs import --input ros2_foxy.repos src`.
*  run `build.sh` script (it invokes `colcon_build` with `--merge-install` argument). Note that merge install is used to simplify libraries installation.
*  if you wish to test the build:
  * run `colcon test --merge-install --package-select ros2cs_core`. You can also use `test.sh` script.
  * run a manual test with basic listener/publisher examples - `ros2cs_talker` and `ros2cs_listener`.
  * run a manual performance test - `ros2cs_performance_talker` and `ros2cs_performance_listener`.

#### Standalone version

By default, build also deploys standalone libraries in `install/standalone` directory. You can turn this feature off by setting cmake option `STANDALONE_BUILD` to `OFF`.

To run standalone application you must deploy it with libraries from:

* on linux, both `install/lib` and `install/standalone`,
* on windows, both `install/bin` and `install/standalone`.

For convenience, deploy scripts (`deploy_unity_plugins.sh` for Linux and `deploy_unity_plugins.ps1`) for Windows are attached.

Note that examples are not meant to be ran with the standalone build, but you can follow the instruction below to do so.

##### Running examples with standalone build

Libraries are loaded dynamically at runtime and need to be in a visible path You should also modify OS environment variables so your executable will find all the required libraries:

* on Linux: `LD_LIBRARY_PATH`,
* on Windows: `PATH` (if executable lies in a different directory than libraries).

Example for linux (make sure `ros2cs_examples` package is built):

```bash
cd install/lib/ros2cs_examples
LD_LIBRARY_PATH=$LD_LIBRARY_PATH:`pwd`/../../standalone:`pwd`/.. ./ros2cs_talker
```

### Generating custom messages

After cloning the project and importing .repos, you can simply put your message package next to other packages in the ros2 sub-folder. Then, build your project and you have all messages generated! You can also modify and use the `custom_message.repos` template to automate the process.
