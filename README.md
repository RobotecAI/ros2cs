ROS2 for C#
=============

A C# module for ROS2, including C# implementation of rcl APIs, message generation, libraries export for Unity, tests and examples.

### Prerequisites

*  ROS2 dashing installed on the system
*  Ubuntu 18.04 (not tested on other platforms)
*  vcs_tools package
*  .NET core - (see here)[https://www.microsoft.com/net/learn/get-started]

### Building

You need to source your ROS2 installation (e.g. `source /opt/ros/dashing/setup.bash`) before you proceed, for each new open terminal. You can also include this command in your ~/.bashrc file.

*  Clone this project
*  Navigate to the top project folder and run the `get_repos.sh` script (it will use vcs_tools to download required ros2 and custom message - if any - packages).
*  Run `build.sh`script (it invokes colcon_build). Note that the --merge-install argument in the build command is important since the Unity plugins script relies on that,
*  If you wish to test the build, run attached listener/publisher examples - ros2cs_talker and ros2cs_listener.
*  If you wish to deploy the libraries to your Unity project, edit and then invoke `deploy_unity_plugins.sh`. You will need to replace the path with your location. Note that this requires Unity-site Robotec asset to work.