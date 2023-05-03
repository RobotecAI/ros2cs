#!/bin/bash

display_usage() {
    echo "Usage: "
    echo "build.sh [--with-tests] [--standalone] [--with-examples]"
    echo ""
    echo "Options:"
    echo "--with-tests - build with tests."
    echo "--standalone - standalone version"
    echo "--with-examples - built with examples"
}

if [ -z "${ROS_DISTRO}" ]; then
    echo "Source your ros2 distro first (galactic, humble or rolling)"
    exit 1
fi

TESTS=0
STANDALONE=OFF
PACKAGES="ros2cs_core"
MSG="Build started."

while [[ $# -gt 0 ]]; do
  key="$1"
  case $key in
    -t|--with-tests)
      TESTS=1
      PACKAGES="$PACKAGES ros2cs_tests"
      MSG="$MSG (with tests)"
      shift # past argument
      ;;
    -s|--standalone)
      STANDALONE=ON
      MSG="$MSG (standalone)"
      shift # past argument
      ;;
    -e|--with-examples)
      PACKAGES="$PACKAGES ros2cs_examples"
      MSG="$MSG (with examples)"
      shift # past argument
      ;;
    -h|--help)
      display_usage
      exit 0
      ;;
    *)    # unknown option
      shift # past argument
      ;;
  esac
done

echo "$MSG"
colcon list --names-only --base-paths src/custom_packages \
| xargs -P 1 -d "\n" colcon build \
--merge-install \
--event-handlers console_direct+ \
--cmake-args \
-DCMAKE_BUILD_TYPE=Release \
-DSTANDALONE_BUILD=$STANDALONE \
-DBUILD_TESTING=$TESTS \
-DCMAKE_SHARED_LINKER_FLAGS="-Wl,-rpath,'\$ORIGIN',-rpath=.,--disable-new-dtags" \
--no-warn-unused-cli \
--packages-up-to $PACKAGES