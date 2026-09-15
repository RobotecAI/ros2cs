# ROS 2 Lyrical source build

## Status and scope

This branch contains Lyrical adaptation fixes tested against real x86_64 Windows and Linux environments. See `docs/lyrical-acceptance-20260915.md` for the exact builds, test results and remaining boundaries. Unity/IL2CPP is a separate, unvalidated target. No prebuilt DLL, SO, or Unity package is committed to the source repository.

The target is x86_64 Linux with a sourced ROS 2 Lyrical installation, or x86_64 Windows 11 with ROS 2 Lyrical and the VS 2022 x64 native tools environment. Ubuntu 26.04 is Lyrical's Tier 1 Ubuntu platform. This is not a claim that Unity Editor has certified support for that Ubuntu version. Use a full repository checkout; a patch/source-subset delivery must first be restored with its `restore_full_sources.py`.

The C# libraries and generated message/service assemblies target `netstandard2.0` for Unity compatibility on both Windows and Linux. Standalone examples and tests target `net10.0`. The machine needs the .NET 10 SDK, Git, vcstool, colcon and CMake 3.21 or later. Linux builds need the native C/C++ toolchain; standalone packaging additionally needs `objdump` and `patchelf`. Windows standalone packaging requires `dumpbin` on PATH and the corresponding Visual C++ runtime installed on the destination machine. NuGet and source dependency downloads require network access on first build.

## Source dependencies

The test executables need the x64 .NET 10 runtime (included with the .NET 10 SDK). Use `dotnet --list-runtimes` to verify it. Python tooling can be installed with `uv` in a `.venv` using the same Python version as the ROS underlay; include `typing_extensions` when running the rclpy interoperability peer.

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

The two commands represent **different build profiles**: run them in separate fresh workspaces, not consecutively in the same build/install tree. A profile contains the distribution, OS, architecture, RMW, standalone setting, library/executable target frameworks and Release configuration. An incompatible cached profile fails before compilation. To reuse a source checkout with another profile, explicitly remove its generated `build`, `install`, and `log` directories after retaining any needed output.

Windows, from the ROS-sourced VS 2022 x64 environment:

```powershell
python tools/lyrical.py build --with-tests --standalone --cmake-generator Ninja
```

The wrappers remain available:

```bash
./build.sh --with-tests --standalone
```

```powershell
.\build.ps1 -with_tests -standalone
```

Install Ninja and use a short workspace path on Windows. The driver enables UTF-8 Python I/O. If building from a `subst` drive, retain the mapping until all builds/tests finish and disable shared C# compilation (`UseSharedCompilation=false`) so an existing compiler server cannot lose access to the mapped path. The generator is part of the cached build profile.

`--with-tests` invokes `colcon build`, `colcon test --merge-install --return-code-on-test-failure`, and `colcon test-result --verbose`. Test results are collected even when the test command fails. The driver checks that the native binding and the common/core managed assemblies exist before reporting an install result. To run just the ros2cs runtime suite in an existing merged installation, use `colcon test --merge-install --packages-select ros2cs_tests --return-code-on-test-failure`.

## Middleware

The default is `rmw_fastrtps_cpp`. To select Cyclone DDS, install the Lyrical `rmw_cyclonedds_cpp` package in the ROS underlay and use a separate build profile:

```bash
python3 tools/lyrical.py build --standalone --rmw rmw_cyclonedds_cpp
```

Each standalone artifact carries one selected RMW. This branch does not implement Zenoh or Connext packaging. The selected RMW is recorded in `install/lyrical-build.json` and `install/standalone/runtime-manifest.json`. Lyrical C# message/service bindings use the generic `rosidl_typesupport_c` dispatcher; concrete DDS typesupport plugins remain runtime dependencies.

## Standalone library collection

Lyrical builds link the native core and generated message/service bindings to
exported CMake runtime targets; the removed `ament_target_dependencies` macro
is used only by the legacy-distribution path. Missing generated dependency
targets fail during configuration.

`tools/collect_runtime.cmake` resolves actual ELF/PE import dependencies. The build driver explicitly includes the selected RMW, ROS logging, and installed typesupport plugins because they can be loaded dynamically rather than appear in an import table. Search roots are ordered with the workspace install first, followed by sourced
overlays and the underlay. A same-basename library in those explicit roots is
selected by that order; selected replacements are scanned again for their own
transitive imports. Missing dependencies and conflicting basenames outside
those roots abort packaging. This is a standard ROS overlay policy, not a check
that arbitrary ABI-incompatible libraries can be mixed. Library versions are taken from the installed binaries rather than OpenSSL or DDS version strings in the build script.

On Linux, deployed libraries receive an `$ORIGIN` RPATH. Shared-library aliases are materialized as ordinary files, retaining SONAME filenames through ZIP extraction and Unity import. System loader/glibc libraries are excluded. On Windows, API-set imports and System32 dependencies remain OS/runtime prerequisites. This is **not a completely self-contained operating-system image**: use a matching OS/runtime environment and review third-party redistribution licenses for deployment. Security extensions, custom RMW plugins and other optional plugins need separate acceptance tests and are not certified by an import-table scan.

The C binding includes x86_64 size/offset assertions for the layouts mirrored by `NativeTypes.cs`. A failure indicates an ABI layout mismatch and must be investigated; do not disable these assertions to make compilation pass. Passing them does not prove the absence of other ABI or semantic changes.

For deployment, preserve the `share` directory included in `install/standalone`, copy the managed assemblies from `install/lib/dotnet`, and set `AMENT_PREFIX_PATH` to the deployment directory. The Lyrical buffer registry needs the bundled `rosidl_buffer_backend` package metadata even for ordinary node initialization. Set `RMW_IMPLEMENTATION` to the packaged RMW; add the deployment directory to `PATH` on Windows or `LD_LIBRARY_PATH` on Linux. Custom buffer backend plugins require their own libraries and plugin manifests and are outside this default profile.

## Local driver tests

```bash
python3 -m unittest discover -s tests/lyrical -v
```

These tests cover orchestration, failure paths, overlay precedence, generated
runtime target linkage and real locally compiled ELF/PE dependency fixtures.
The CMake fixture compiles a small C library against test interface targets;
it does not substitute for configuring actual ROS packages. Native fixture
tests skip explicitly if their toolchain is unavailable. On Windows, run from
a native tools shell with `cl`, `dumpbin`, `nmake` and `cmake` on PATH.
See `tests/lyrical/README.md` for the recorded checks and remaining acceptance
work. These tests do **not** compile ROS packages or exercise a DDS network.

## Required acceptance tests

Compile against the actual Lyrical underlay, run all ros2cs tests, and verify standard/custom message generation. Test publication/subscription, service request/response, shutdown/reinitialization, and disposal under both selected RMW profiles. Test a standalone process from a fresh unsourced shell. Verify missing-library diagnostics and distribution mismatch rejection. Windows PE collection and MSVC compilation require a separate Windows run. IL2CPP, actions, ARM, macOS, and advanced DDS security capabilities are not newly implemented or validated here.

## Primary references

- ROS 2 Lyrical platforms: https://docs.ros.org/en/lyrical/Installation.html
- Official Lyrical source manifest: https://github.com/ros2/ros2/blob/lyrical/ros2.repos
- ROSIDL Python generator compatibility: https://github.com/ros2/rosidl/blob/lyrical/rosidl_cmake/rosidl_cmake/__init__.py
- Baseline: https://github.com/RobotecAI/ros2cs/tree/e08651b64fe9868c0df9d98d81e77fc40b8d5c92
