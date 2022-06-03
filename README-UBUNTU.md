# ros2cs - Ubuntu 20.04 and Ubuntu 22.04

## Building

### Prerequisites

**General**

- ROS2 installed on the system, along with `test-msgs`, `cyclonedds` and `fastrtps` packages
- vcstool package - [see here](https://github.com/dirk-thomas/vcstool)


```bash
# Install rmw and tests-msgs for your ROS2 distribution
apt install -y ros-${ROS_DISTRO}-test-msgs
apt install -y ros-${ROS_DISTRO}-fastrtps ros-${ROS_DISTRO}-rmw-fastrtps-cpp
apt install -y ros-${ROS_DISTRO}-cyclonedds ros-${ROS_DISTRO}-rmw-cyclonedds-cpp

# Install vcstool package
curl -s https://packagecloud.io/install/repositories/dirk-thomas/vcstool/script.deb.sh | sudo bash
sudo apt-get update
sudo apt-get install -y python3-vcstool

# Install Microsoft packages
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

**Ubuntu 20.04 only**

- .NET core 3.1 sdk - [see here](https://www.microsoft.com/net/learn/get-started)

```
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-3.1
```

**Ubuntu 22.04 only**

- mono - [see here](https://www.mono-project.com/)

```
sudo apt-get update; \
  sudo apt install mono-complete
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
