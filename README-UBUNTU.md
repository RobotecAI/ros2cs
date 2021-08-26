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
- Navigate to the top project folder and pull required repositories (`./get_repos.sh`)
  - You can run `get_repos` script with `--get-custom-messages argument` to fetch extra messages from custom_messages.repos file.
  - It will use `vcstool` to download required ROS2 packages. By default, this will get repositories as set in `${ROS_DISTRO}`.
- Build package (`./build.sh`)
  - It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
  - You can build tests by adding `--with-tests` argument to command.
- To test your build please check main readme [Testing section](README.md#testing)
