# ROS2CS - Ubuntu

## Building

### Prerequisites

**General**

- ROS2 installed on the system, along with `test-msgs`, `cyclonedds` and `fastrtps` packages
- vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
- .NET 10 SDK - [see here](https://learn.microsoft.com/dotnet/core/install/linux-ubuntu)


```bash
# Install rmw and tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs
apt install -y ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp
apt install -y ros-${ROS_DISTRO}-cyclonedds ros-${ROS_DISTRO}-rmw-cyclonedds-cpp

# Install vcstool package
curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
sudo apt-get update
sudo apt-get install -y python3-vcstool

# Install .NET 10 SDK using Microsoft's installer (requires a supported Linux OS).
curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --channel 10.0
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet --version
```

**Optional**

- `patchelf` tool for standalone version builds

```bash
sudo apt install patchelf
```

### Steps

- Clone this project
- Source your ROS2 installation
  ```bash
  # Change foxy to whatever version you are using
  source /opt/ros/foxy/setup.bash
  ```
- Navigate to the top project folder and pull required repositories
  ```bash
  ./get_repos.sh
  ```
  - You can run `get_repos` script with `--get-custom-messages argument` to fetch extra messages from `custom_messages.repos` file.
  - It will use `vcstool` to download required ROS2 packages. By default, this will get repositories as set in `ros2_${ROS_DISTRO}.repos`.
- Build package in _overlay_ mode:
  ```bash
  ./build.sh
  ```
  or to build a _standalone_ version:
  ```bash
  ./build.sh --standalone
  ```
  - It invokes `colcon_build` with `--merge-install` argument to simplify libraries installation.
  - You can build tests by adding `--with-tests` argument to command.
- To test your build please check main readme [Testing section](README.md#testing)
