# ROS 2 Lyrical source build

## Status and scope

This branch contains **source-level Lyrical adaptation work awaiting target-environment validation**. The local delivery tests do not establish that ROS 2 Lyrical, Windows, or Unity builds succeed. No prebuilt DLL, SO, or Unity package is included.

The target is x86_64 Linux with a sourced ROS 2 Lyrical installation, or x86_64 Windows 11 with ROS 2 Lyrical and the VS 2022 x64 native tools environment. Ubuntu 26.04 is Lyrical's Tier 1 Ubuntu platform. This is not a claim that Unity Editor has certified support for that Ubuntu version. Use a full repository checkout; a patch/source-subset delivery must first be restored with its `restore_full_sources.py`.

The C# libraries keep the upstream `netstandard2.0` target. The machine needs a .NET SDK capable of restoring and building that target, Git, vcstool, colcon and CMake 3.21 or later. Linux builds need the native C/C++ toolchain; standalone packaging additionally needs `objdump` and `patchelf`. Windows standalone packaging requires `dumpbin` on PATH and the corresponding Visual C++ runtime installed on the destination machine. NuGet and source dependency downloads require network access on first build.

## Source dependencies

With ROS 2 Lyrical sourced, run from this repository:

```bash
python3 tools/lyrical.py fetch
```

Windows uses the same command with `python`. The `get_repos.sh` and `get_repos.ps1` entrypoints invoke this driver when `ROS_DISTRO=lyrical`.

`ros2_lyrical.repos` declares the ROS interface and build-tool sources. `src/lyrical-dependencies.lock.repos` records the exact downloaded revisions after a successful import. `src/.lyrical-dependencies.json` tracks an interrupted or completed import. Existing unstamped ROS dependencies are rejected; the driver does not reset repositories, discard changes, or replace other distributions' source trees.

Custom message imports are explicit:

```bash
python3 tools/lyrical.py fetch --get-custom-messages
```

The source checkout and ROS underlay must use the same distribution. Install missing ROS build dependencies reported by colcon/rosdep in that Lyrical environment; this delivery does not include the ROS underlay or vendored dependency source code.

## Build

Linux:

```bash
source /opt/ros/lyrical/setup.bash
python3 tools/lyrical.py build --with-tests
python3 tools/lyrical.py build --with-tests --standalone
```

The two commands represent **different build profiles**: run them in separate fresh workspaces, not consecutively in the same build/install tree. A profile contains the distribution, OS, architecture, RMW, standalone setting and Release configuration. An incompatible cached profile fails before compilation. To reuse a source checkout with another profile, explicitly remove its generated `build`, `install`, and `log` directories after retaining any needed output.

Windows, from the ROS-sourced VS 2022 x64 environment:

```powershell
python tools/lyrical.py build --with-tests --standalone
```

The wrappers remain available:

```bash
./build.sh --with-tests --standalone
```

```powershell
.\build.ps1 -with_tests -standalone
```

`--with-tests` invokes `colcon build`, `colcon test --return-code-on-test-failure`, and `colcon test-result --verbose`. A nonzero process exit code aborts the pipeline. The driver checks that the native binding and the common/core managed assemblies exist before reporting an install result.

## Middleware

The default is `rmw_fastrtps_cpp`. To select Cyclone DDS, install the Lyrical `rmw_cyclonedds_cpp` package in the ROS underlay and use a separate build profile:

```bash
python3 tools/lyrical.py build --standalone --rmw rmw_cyclonedds_cpp
```

Each standalone artifact carries one selected RMW. This branch does not implement Zenoh or Connext packaging. The selected RMW is recorded in `install/lyrical-build.json` and `install/standalone/runtime-manifest.json`. Source-generated typesupport libraries are still generated according to the installed ROS typesupports.

## Standalone library collection

`tools/collect_runtime.cmake` resolves actual ELF/PE import dependencies. The build driver explicitly includes the selected RMW, ROS logging, and installed typesupport plugins because they can be loaded dynamically rather than appear in an import table. Missing dependencies and conflicting basenames abort packaging. Library versions are taken from the installed binaries rather than OpenSSL or DDS version strings in the build script.

On Linux, deployed libraries receive an `$ORIGIN` RPATH. Shared-library aliases are materialized as ordinary files, retaining SONAME filenames through ZIP extraction and Unity import. System loader/glibc libraries are excluded. On Windows, API-set imports and System32 dependencies remain OS/runtime prerequisites. This is **not a completely self-contained operating-system image**: use a matching OS/runtime environment and review third-party redistribution licenses for deployment. Security extensions, custom RMW plugins and other optional plugins need separate acceptance tests and are not certified by an import-table scan.

The C binding includes x86_64 size/offset assertions for the layouts mirrored by `NativeTypes.cs`. A failure indicates an ABI layout mismatch and must be investigated; do not disable these assertions to make compilation pass. Passing them does not prove the absence of other ABI or semantic changes.

## Local driver tests

```bash
python3 -m unittest discover -s tests/lyrical -v
```

These tests cover orchestration, failure paths, dependency names, metadata-related inputs and real locally compiled ELF dependency fixtures. They do **not** compile ROS packages or exercise a DDS network.

## Required acceptance tests

Compile against the actual Lyrical underlay, run all ros2cs tests, and verify standard/custom message generation. Test publication/subscription, service request/response, shutdown/reinitialization, and disposal under both selected RMW profiles. Test a standalone process from a fresh unsourced shell. Verify missing-library diagnostics and distribution mismatch rejection. Windows PE collection and MSVC compilation require a separate Windows run. IL2CPP, actions, ARM, macOS, and advanced DDS security capabilities are not newly implemented or validated here.

## Primary references

- ROS 2 Lyrical platforms: https://docs.ros.org/en/lyrical/Installation.html
- Official Lyrical source manifest: https://github.com/ros2/ros2/blob/lyrical/ros2.repos
- ROSIDL Python generator compatibility: https://github.com/ros2/rosidl/blob/lyrical/rosidl_cmake/rosidl_cmake/__init__.py
- Baseline: https://github.com/RobotecAI/ros2cs/tree/e08651b64fe9868c0df9d98d81e77fc40b8d5c92
