Ros2cs
=============

A C# .NET library for ROS2, including C# implementation of rcl APIs, message generation, tests and examples.

Ros2cs is also an independent part of [Ros2 For Unity](https://github.com/RobotecAI/ros2-for-unity), which enables high-performance communication between simulation and ROS2 robot packages. Follow instructions there instead if you are intending to use ros2cs with Unity3D. 

### Features

- A set of core abstractions such as Node, Publisher, Subscription, QoS, Clock
- Comes with support for all standard ros2 messages
- Custom messages can be easily generated from unmodified ROS2 packages
- A logger that can be hooked to your application callbacks (e.g. in Unity3D)

### Platforms

Supported OSes: 
- Ubuntu 20.04  (bash)
- Windows 10 (powershell)

Supported ROS2 distributions:
- Foxy
- Galactic

On Windows, ros2cs libraries can be built in two flavors:
- standalone (no ROS2 installation required on target machine, e.g. your Unity3D simulation server). All required dependencies are installed and can be used e.g. as a complete set of Unity3D plugins.
- overlay (assuming existing (supported) ROS2 installation on target machine). Only ros2cs libraries and generated messages are installed.

### Prerequisites

#### Ubuntu

*  ROS2 installed on the system, along with `test-msgs` and `fastrtps` packages
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET core 3.1 sdk - [see here](https://www.microsoft.com/net/learn/get-started)

The following script can be used to install the aforementioned prerequisites on Ubuntu 20.04:

```bash
# Install tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp

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

#### Windows

*  ROS2 installed on the system
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET 5.0 sdk - [see here](https://dotnet.microsoft.com/download/dotnet/5.0)
*  xUnit testing framework (for tests only) - [see here](https://xunit.net/)

### Building

> For **Windows**, [path length is limited to 260 characters](https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation). Clone your repo to `C:\dev` or a similar shallow path to avoid this issue during build.

> For **Windows**, a Visual Studio preconfigured powershell terminal must be used. Standard powershell prompt might not be configured properly to be used with MSVC compiler and Windows SDKs.  You should have Visual Studio already installed (ROS2 dependency) and you can find shortcut for `Developer PowerShell for VS` here: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Visual Studio 2019\Visual Studio Tools`. 

> A powershell terminal with administrator privileges is required for **Windows** and **ros2 galactic**. This is because python packages installation requires a privilage for creating symlinks. More about this issue: [github issue](https://github.com/ament/ament_cmake/issues/350).

> There is a bug with hardcoded include exports in some **ros2 galactic** packages on **Windows**. Easiest workaround is to create a `C:\ci\ws\install\include` directory in your system. More about this bug and proposed workarounds: [github issue](https://github.com/ros2/rclcpp/issues/1688#issuecomment-858467147).

You need to source your ROS2 installation (e.g. `source /opt/ros/foxy/setup.bash` on Ubuntu or `C:\dev\ros2_foxy\local_setup.ps1` on Windows) before you proceed, for each new open terminal. On Ubuntu, you can also include this command in your `~/.profile` file.

*  Clone this project.
*  Navigate to the top project folder and run the `get_repos.sh` (Ubuntu) or `get_repos.ps1` (Windows) script.
   * You can run `get_repos` script with `--get-custom-messages` argument to fetch extra messages from `custom_messages.repos` file.
   * It will use `vcstool` to download required ROS2 packages. By default, this will get repositories as set in ${ROS_DISTRO}.
*  Run `build.sh` (Ubuntu) or `build.ps1` (Windows) script.
   * You can build tests by adding `--with-tests` argument to `build` command.
   * It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
* If you wish to test the build:
  * Make sure your NuGet repositories can resolve `xUnit` dependency. You can call `dotnet nuget list source` to see your current sources for NuGet packages. Please note that `Microsoft Visual Studio Offline Packages` is usually insufficient. You can fix it by adding `nuget.org` repository: `dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json`.
  * Make sure you have tests built (`build.sh --with-tests` (Linux) or `build.ps1 --with-tests`).
  * Run `test.sh` (Ubuntu) or `test.ps1` (Windows) script.
  * Run a manual test with basic listener/publisher examples - `ros2cs_talker` and `ros2cs_listener`.
  * Run a manual performance test - `ros2cs_performance_talker` and `ros2cs_performance_listener`.

#### Standalone version (Windows)

On Windows, by default, build process generates standalone libraries in `install/standalone` directory.
You can disable this feature by setting CMake option `STANDALONE_BUILD` to `OFF`.

To run standalone application you must deploy it with libraries from both `install/bin` and `install/standalone`.

By default, examples are ran with the overlay build, but you can follow the instruction below to run them with the standalone build (e.g. to test it).

##### Running examples with standalone build

Libraries are loaded dynamically at runtime and need to be in a visible path. You should also modify `PATH`  environment variable so your executable will find all the required libraries (if executable lies in a different directory than libraries).

### Generating custom messages

After cloning the project and importing .repos, you can simply put your message package next to other packages in the `src/ros2` sub-folder. Then, build your project and you have all messages generated! You can also modify and use the `custom_message.repos` template to automate the process with `get_repos` script.

## Troubleshooting

### Tests are not working ('charmap' codec can't decode byte) on Windows

Problem may occur on non english version of Windows. This error is caused by impossibility in decoding `dotnet` output by ament tools.

**Fix**: Change your `dotnet` output to english by temporarily renaming your localization directory (`pl` to `pl.bak`, `fr` to `fr.bak` etc.) in your `dotnet` sdk directory.

## Acknowledgements 

The project started as a fork of [ros2_dotnet](https://github.com/ros2-dotnet/ros2_dotnet) but moved away from its root through new features and design choices. Nevertheless, ros2cs is built on foundation of open-source efforts of Esteve Fernandez (esteve), Lennart Nachtigall (firesurfer), Samuel Lindgren (samiamlabs) and other contributors to ros2_dotnet project.

Open-source release of ros2cs was made possible through cooperation with Tier IV. Thanks to encouragement, support and requirements driven by Tier IV the project was significantly improved in terms of portability, stability, core structure and user-friendliness.
