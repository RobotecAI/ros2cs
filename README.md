Ros2cs
=============

A C# .NET library for ROS2, including C# implementation of rcl APIs, message generation, tests and examples.

Ros2cs is an independent part of Ros2 For Unity, which enables high-performance communication between simulation and ROS2 robot packages.

### Features

- A set of core abstractions such as Node, Publisher, Subscription, QoS, Clock
- Comes with support for all standard ros2 messages
- Custom messages can be easily generated from unmodified ROS2 packages
- A logger that can be hooked to your application callbacks (e.g. in Unity3D)

### Platforms

Supported OSes: 
- Ubuntu 20.04
- Windows 10

Supported ROS2 distributions:
- Foxy
- Galactic

Ros2cs libraries can be built in two flavors:
- standalone (no ROS2 installation required on target machine, e.g. your Unity3D simulation server). All required dependencies are installed and can be used e.g. as a complete set of Unity3D plugins.
- overlay (assuming existing (supported) ROS2 installation on target machine). Only ros2cs libraries and generated messages are installed.

### Prerequisites

*  ROS2 installed on the system, along with `test-msgs` package (e.g. for Foxy: `ros-foxy-test-msgs`).
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core 3.1 - [see here](https://www.microsoft.com/net/learn/get-started)

The following script can be used to install the aforementioned prerequisites on Ubuntu 20.04:

```bash
# Install tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs

# Install vcstool package
curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
sudo apt-get update
sudo apt-get install -y python3-vcstool

# Install .NET core
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
```

### Building

You need to source your ROS2 installation (e.g. `source /opt/ros/foxy/setup.bash` on Ubuntu) before you proceed, for each new open terminal. You can also include this command in your `~/.bashrc` file.

*  Clone this project.
*  Navigate to the top project folder and run the `get_repos.sh` script.
   * It will use `vcstool` to download required ROS2 and custom message packages (if any).
     By default, this will get repositories as set in ${ROS_DISTRO}.
   * **Note that on Windows the command is a bit different**: `vcs import --input ros2_foxy.repos src`.
*  Run `build.sh` script.
   * It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
* If you wish to test the build:
  * Run `test.sh` script.
  * Run a manual test with basic listener/publisher examples - `ros2cs_talker` and `ros2cs_listener`.
  * Run a manual performance test - `ros2cs_performance_talker` and `ros2cs_performance_listener`.

#### Standalone version

By default, build process generates standalone libraries in `install/standalone` directory.
You can disable this feature by setting CMake option `STANDALONE_BUILD` to `OFF`.

To run standalone application you must deploy it with libraries from:

* on linux, both `install/lib` and `install/standalone`,
* on windows, both `install/bin` and `install/standalone`.

For convenience, deploy scripts (`deploy_unity_plugins.sh` for Linux and `deploy_unity_plugins.ps1`) for Windows are attached.

By default, examples are ran with the overlay build, but you can follow the instruction below to run them with the standalone build (e.g. to test it).

##### Running examples with standalone build

Libraries are loaded dynamically at runtime and need to be in a visible path. You should also modify OS environment variables so your executable will find all the required libraries:

* on Linux: `LD_LIBRARY_PATH`,
* on Windows: `PATH` (if executable lies in a different directory than libraries).

Example for linux (make sure `ros2cs_examples` package is built):

```bash
cd install/lib/ros2cs_examples
LD_LIBRARY_PATH=$LD_LIBRARY_PATH:`pwd`/../../standalone:`pwd`/.. ./ros2cs_talker
```

### Generating custom messages

After cloning the project and importing .repos, you can simply put your message package next to other packages in the ros2 sub-folder. Then, build your project and you have all messages generated! You can also modify and use the `custom_message.repos` template to automate the process.
