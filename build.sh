#!/bin/bash

# Lyrical uses the checked, cross-platform build driver.
if [ "${ROS_DISTRO:-}" = "lyrical" ]; then
  SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
  exec python3 "$SCRIPT_ROOT/tools/lyrical.py" build "$@"
fi

display_usage() {
    echo "Usage: "
    echo "build.sh [--with-tests] [--standalone]"
    echo ""
    echo "Options:"
    echo "--with-tests - build with tests."
    echo "--standalone - standalone version"
}

if [ -z "${ROS_DISTRO}" ]; then
    echo "Source your ros2 distro first (foxy, galactic, humble, jazzy or rolling are supported)"
    exit 1
fi

TESTS=0
MSG="Build started."
STANDALONE=OFF

while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    -t|--with-tests)
      TESTS=1
      MSG="$MSG (with tests)"
      shift # past argument
      ;;
    -s|--standalone)
      STANDALONE=ON
      MSG="$MSG (standalone)"
      shift # past argument
      ;;
    -h|--help)
      display_usage
      exit 0
      shift # past argument
      ;;
    *)    # unknown option
      shift # past argument
      ;;
  esac
done

echo $MSG
colcon build \
--merge-install \
--event-handlers console_direct+ \
--cmake-args \
-DCMAKE_BUILD_TYPE=Release \
-DSTANDALONE_BUILD=$STANDALONE \
-DBUILD_TESTING=$TESTS \
-DCMAKE_SHARED_LINKER_FLAGS="-Wl,-rpath,'\$ORIGIN',-rpath=.,--disable-new-dtags" \
--no-warn-unused-cli
