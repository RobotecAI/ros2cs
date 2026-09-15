# Lyrical regression checks

For the .NET 10 SDK migration with Unity-compatible `netstandard2.0` libraries,
see [the Windows/Linux validation results](NET10-VALIDATION.md).

Run `python -m unittest discover -s tests/lyrical -v` from this repository.
CMake 3.21+ is required for runtime dependency checks. Linux native fixtures
require GCC, objdump and CMake. Windows fixtures require the x64 native tools
shell (cl, dumpbin, nmake) and CMake. Missing tools cause explicit skips.

## 2026-09-14 implementation batch

- Imported the source adaptation based on Jazzy commit
  `e08651b64fe9868c0df9d98d81e77fc40b8d5c92`.
- Replaced removed ament calls on the Lyrical core and message/service paths
  with exported runtime targets; retained the legacy distribution path.
- Declared the C runtime dependencies and included visibility headers directly.
- Selected standalone binaries by workspace/overlay/underlay search order,
  rescanning selected replacements for transitive dependencies. Unrelated
  collisions and missing dependencies still fail.
- Used CMake install RPATH properties for `$ORIGIN`, avoiding shell expansion
  of a raw linker flag on Linux. Standalone copies are still patched before
  publication.

Validation machine: Windows x64, Python 3.13.15, CMake 4.4.3, MSVC from Visual
Studio 2026. Unit/native fixture suite: 26 passed, 3 Linux ELF tests skipped.
The suite includes real PE transitive lookup, missing-DLL rejection, overlay
selection, and compilation of a C fixture with generated dependency targets.
The missing-DLL test intentionally prints a CMake error before passing.

A separate smoke project compiled the core/common C# sources together with
simple Lyrical-generated message and service sources targeting netstandard2.0
using .NET SDK 10.0.401: 0 errors, 5 existing unused/unassigned-field warnings.
That combined assembly is a source compilation check, not the official
colcon-built assemblies or a distributable package.

The ROSIDL smoke used official Lyrical source commit
`3983d007c1cfde197c63002635c7e45177939aec`, EmPy 4.2.1 and Lark 1.3.1.
The message includes an integer, string and double sequence; the service has
an integer request and boolean response. This does not cover every IDL type.

## Still required before claiming runtime support

A full build against installed ROS 2 Lyrical headers/libraries, ABI assertion
compilation, official separate managed assembly generation, ros2cs tests,
standard/custom messages, publication/subscription, service round trips,
shutdown/reinitialization, and clean-process standalone loading. Validate
Fast DDS and Cyclone DDS in separate workspaces. Run the ELF tests on Linux
and validate the officially supported Windows toolchain separately from the
MSVC 2026 fixtures used here. Unity integration is a separate repository/task.
