# ROS2CS - Ubuntu 20.04

## Building

### Prerequisites

- ROS2 installed on the system, along with `test-msgs` and `fastrtps` packages
- vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
- .NET core 3.1 sdk - [see here](https://www.microsoft.com/net/learn/get-started)

The following script can be used to install the aforementioned prerequisites on Ubuntu 20.04:

```bash
# Install tests-msgs for your ROS2 distribution
sudo apt install -y ros-${ROS_DISTRO}-test-msgs ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp

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

### Steps

- Clone this project
- Source your ROS2 installation (`source /opt/ros/foxy/setup.bash`)
- Pull required repositories (`./get_repos.sh`)
- Build package (`./build.sh`)

-----

For more in-depth build instructions and package usage examples please refer to [Ros2 For Unity](https://github.com/RobotecAI/ros2-for-unity) repository.
