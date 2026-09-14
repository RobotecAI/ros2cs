#! /bin/bash

# Lyrical uses the checked, cross-platform build driver.
if [ "${ROS_DISTRO:-}" = "lyrical" ]; then
  SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
  exec python3 "$SCRIPT_ROOT/tools/lyrical.py" fetch "$@"
fi

if [ -z "${ROS_DISTRO}" ]; then
    echo "Can't detect ROS2 version. Source your ros2 distro first."
    exit 1
fi

echo "Detected ROS2 ${ROS_DISTRO}. Getting required repos from 'ros2_"${ROS_DISTRO}".repos'"
vcs import src < ros2_"${ROS_DISTRO}".repos

if [ "$1" = "--get-custom-messages" ]; then
    echo -e "\nGetting custom messages from 'custom_messages.repos'."
    vcs import src < custom_messages.repos
fi
