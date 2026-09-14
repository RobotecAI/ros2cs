#!/usr/bin/env python3
# SPDX-License-Identifier: Apache-2.0
"""Lyrical source/build driver. Requires a full ros2cs checkout and a sourced ROS installation."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shutil
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
RMWS = ("rmw_fastrtps_cpp", "rmw_cyclonedds_cpp")


class BuildError(RuntimeError):
    pass


def run(args, cwd=None, capture=False):
    print("+ " + subprocess.list2cmdline([str(x) for x in args]), flush=True)
    result = subprocess.run([str(x) for x in args], cwd=cwd, check=True,
                            text=True, stdout=subprocess.PIPE if capture else None)
    return result.stdout if capture else None


def require_tools(*tools):
    missing = [name for name in tools if shutil.which(name) is None]
    if missing:
        raise BuildError("Missing tools on PATH: " + ", ".join(missing))


def check_environment(rmw=None):
    if os.environ.get("ROS_DISTRO") != "lyrical":
        raise BuildError("Source a ROS 2 Lyrical installation first (ROS_DISTRO must be lyrical).")
    if platform.system() not in ("Linux", "Windows"):
        raise BuildError("This Lyrical build target supports Linux and Windows only.")
    if platform.machine().lower() not in ("x86_64", "amd64"):
        raise BuildError("This build target is x86_64; no ARM or 32-bit binaries are produced.")
    selected = rmw or os.environ.get("RMW_IMPLEMENTATION") or RMWS[0]
    if selected not in RMWS:
        raise BuildError("Supported RMW implementations: " + ", ".join(RMWS))
    return selected


def full_checkout(root=ROOT):
    required = root / "src/ros2cs/ros2cs_core/native/rcl_native_interface.c"
    if not required.is_file():
        raise BuildError("This is not a full ros2cs checkout. Run the delivery's restore_full_sources.py first.")


def sha256(path):
    h = hashlib.sha256()
    with Path(path).open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def write_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temp = path.with_suffix(path.suffix + ".tmp")
    temp.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    temp.replace(path)


def fetch(root=ROOT, custom=False):
    check_environment()
    full_checkout(root)
    require_tools("git", "vcs")
    manifest = root / "ros2_lyrical.repos"
    state = root / "src/.lyrical-dependencies.json"
    fingerprint = sha256(manifest)
    existing = root / "src/ros2"
    if existing.exists() and any(existing.iterdir()) and not state.exists():
        raise BuildError("Unstamped ROS dependencies exist in src/ros2. Use a fresh Lyrical checkout; no sources were overwritten.")
    if state.exists() and json.loads(state.read_text())["manifest_sha256"] != fingerprint:
        raise BuildError("The dependency manifest changed. Use a fresh dependency checkout; no sources were overwritten.")
    # Write state before import so an interrupted first import can be resumed.
    write_json(state, {"distro": "lyrical", "manifest_sha256": fingerprint, "complete": False})
    run(["vcs", "import", "--input", manifest, root / "src"], cwd=root)
    # vcstool can leave an already-present repository untouched. Check that a
    # resumed import has not silently kept Jazzy sources.
    if existing.is_dir():
        for checkout in sorted(existing.iterdir()):
            if (checkout / ".git").exists():
                branch = run(["git", "branch", "--show-current"], cwd=checkout, capture=True).strip()
                if branch != "lyrical":
                    raise BuildError("ROS dependency is not on the lyrical branch: " + str(checkout))
    if custom:
        run(["vcs", "import", "--input", root / "custom_messages.repos", root / "src"], cwd=root)
    lock = run(["vcs", "export", "--exact", root / "src"], cwd=root, capture=True)
    (root / "src/lyrical-dependencies.lock.repos").write_text(lock, encoding="utf-8")
    write_json(state, {"distro": "lyrical", "manifest_sha256": fingerprint, "complete": True})


def native_libraries(directory, windows=None):
    windows = platform.system() == "Windows" if windows is None else windows
    result = []
    if not directory.is_dir():
        return result
    for path in directory.iterdir():
        name = path.name.lower()
        if not path.is_file() or "_python" in name or name.endswith("_py.dll"):
            continue
        if windows and name.endswith(".dll"):
            result.append(path)
        elif not windows and re.search(r"\.so(?:\.[0-9]+)*$", name):
            result.append(path)
    return sorted(result)


def search_directories(install):
    prefixes = [install]
    for env in ("AMENT_PREFIX_PATH", "CMAKE_PREFIX_PATH", "COLCON_PREFIX_PATH"):
        prefixes += [Path(p) for p in os.environ.get(env, "").split(os.pathsep) if p]
    dirs = []
    for prefix in prefixes:
        dirs.extend((prefix / "lib", prefix / "bin", prefix / "Library/bin"))
        # Vendor packages can install shared libraries beneath opt/<vendor>/lib.
        if (prefix / "opt").is_dir():
            dirs.extend(sorted((prefix / "opt").glob("*/lib")))
            dirs.extend(sorted((prefix / "opt").glob("*/bin")))
    env = "PATH" if platform.system() == "Windows" else "LD_LIBRARY_PATH"
    dirs += [Path(p) for p in os.environ.get(env, "").split(os.pathsep) if p]
    return list(dict.fromkeys(p.resolve() for p in dirs if p.is_dir()))


def find_library(stem, directories, windows=None):
    windows = platform.system() == "Windows" if windows is None else windows
    for directory in directories:
        patterns = [stem + ".dll"] if windows else ["lib" + stem + ".so", "lib" + stem + ".so.*"]
        for pattern in patterns:
            found = sorted(directory.glob(pattern))
            if found:
                return found[0]
    raise BuildError("Required runtime library not found in the sourced installation: " + stem)


def runtime_seeds(install, directories, rmw):
    folder = install / ("bin" if platform.system() == "Windows" else "lib")
    seeds = native_libraries(folder)
    # These plugins can be dlopen/LoadLibrary-loaded and invisible to the ELF/PE
    # dependency graph. Explicitly seed them rather than assuming DT_NEEDED covers all ROS plugins.
    for name in ("ros2cs_native", "rcl", "rcutils", "rmw_implementation", rmw, "rcl_logging_spdlog"):
        seeds.append(find_library(name, directories))
    for directory in directories:
        for path in native_libraries(directory):
            if "__rosidl_typesupport_" in path.name:
                seeds.append(path)
    return list(dict.fromkeys(p.absolute() for p in seeds))


def cmake_quote(path):
    value = str(path).replace("\\", "/")
    if any(x in value for x in (";", "]===]", "\n", "\r")):
        raise BuildError("Unsupported character in CMake dependency path: " + value)
    return "[===[" + value + "]===]"


def dependency_closure(seeds, directories, work):
    """Resolve ELF/PE imports without executing the libraries being inspected."""
    require_tools("cmake")
    work.mkdir(parents=True, exist_ok=True)
    config = work / "inputs.cmake"
    output = work / "resolved.txt"
    config.write_text("set(SEEDS\n" + "\n".join(cmake_quote(p) for p in seeds) +
                      "\n)\nset(SEARCH_DIRS\n" + "\n".join(cmake_quote(p) for p in directories) +
                      "\n)\nset(OUTPUT_FILE " + cmake_quote(output) + ")\n", encoding="utf-8")
    run(["cmake", "-DINPUT_FILE=" + str(config), "-P", ROOT / "tools/collect_runtime.cmake"])
    return [Path(p) for p in output.read_text(encoding="utf-8").splitlines() if p]


def copy_unique(source, destination):
    """Flatten shared libraries only when colliding names have identical bytes."""
    target = destination / source.name
    if target.exists() and sha256(source) != sha256(target):
        raise BuildError("Conflicting native library basename: " + source.name)
    if not target.exists():
        shutil.copy2(source, target)  # Dereference aliases into real files for portable ZIP/Unity import.
    return target


def bundle(install, rmw):
    require_tools("cmake")
    if platform.system() == "Linux":
        require_tools("objdump", "patchelf")
    else:
        require_tools("dumpbin")  # Run from the VS 2022 x64 Native Tools environment.
    directories = search_directories(install)
    seeds = runtime_seeds(install, directories, rmw)
    target = install / "standalone"
    with tempfile.TemporaryDirectory(prefix="lyrical-runtime-", dir=install) as temp:
        work = Path(temp)
        dependencies = dependency_closure(seeds, directories, work / "scan")
        output = work / "standalone"
        output.mkdir()
        origins = {}
        for source in list(dict.fromkeys(seeds + dependencies)):
            dest = copy_unique(source, output)
            origins.setdefault(dest.name, str(source))
            # Preserve real-file names as well as requested SONAME aliases.
            resolved = source.resolve()
            if resolved.name != source.name:
                dest = copy_unique(resolved, output)
                origins.setdefault(dest.name, str(resolved))
        if platform.system() == "Linux":
            for path in native_libraries(output):
                run(["patchelf", "--set-rpath", "$ORIGIN", path])
        write_json(output / "runtime-manifest.json", {
            "ros_distro": "lyrical", "rmw_implementation": rmw,
            "platform": platform.system(), "architecture": "x86_64",
            "files": [{"name": p.name, "sha256": sha256(p), "source": origins[p.name]}
                      for p in sorted(output.iterdir())],
        })
        if target.exists():
            shutil.rmtree(target)
        shutil.move(str(output), target)
    return target


def build(workspace, standalone=False, with_tests=False, rmw=None):
    selected = check_environment(rmw)
    full_checkout()
    require_tools("colcon", "cmake", "dotnet")
    workspace = Path(workspace).resolve()
    if not (workspace / "src").is_dir():
        raise BuildError("Workspace has no src directory: " + str(workspace))
    os.environ["RMW_IMPLEMENTATION"] = selected
    profile = {"distro": "lyrical", "platform": platform.system(), "architecture": "x86_64",
               "rmw": selected, "standalone": bool(standalone), "configuration": "Release"}
    marker = workspace / "build/lyrical-build-profile.json"
    if marker.exists():
        if json.loads(marker.read_text()) != profile:
            raise BuildError("Build profile changed. Use a fresh workspace or explicitly remove build/install/log before rebuilding.")
    elif any((workspace / p).exists() and any((workspace / p).iterdir()) for p in ("build", "install")):
        raise BuildError("Unstamped build/install artifacts exist. Use a fresh workspace; no artifacts were deleted.")
    write_json(marker, profile)
    args = ["colcon", "build", "--merge-install", "--event-handlers", "console_direct+", "--cmake-args",
            "-DCMAKE_BUILD_TYPE=Release", "-DSTANDALONE_BUILD=OFF",
            "-DBUILD_TESTING=" + ("ON" if with_tests else "OFF")]
    if platform.system() == "Linux":
        args += ["-DCMAKE_SHARED_LINKER_FLAGS=-Wl,-rpath,$ORIGIN,--disable-new-dtags"]
    run(args, cwd=workspace)
    install = workspace / "install"
    native_dir = install / ("bin" if platform.system() == "Windows" else "lib")
    find_library("ros2cs_native", [native_dir])
    for assembly in ("ros2cs_common.dll", "ros2cs_core.dll"):
        if not (install / "lib/dotnet" / assembly).is_file():
            raise BuildError("Build did not produce " + assembly)
    if with_tests:
        run(["colcon", "test", "--return-code-on-test-failure", "--event-handlers", "console_direct+"], cwd=workspace)
        run(["colcon", "test-result", "--verbose"], cwd=workspace)
    if standalone:
        bundle(install, selected)
    write_json(install / "lyrical-build.json", profile)
    return install


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    p = commands.add_parser("fetch")
    p.add_argument("--get-custom-messages", action="store_true")
    p = commands.add_parser("build")
    p.add_argument("--workspace", type=Path, default=ROOT)
    p.add_argument("--standalone", "-s", action="store_true")
    p.add_argument("--with-tests", "-t", action="store_true")
    p.add_argument("--rmw", choices=RMWS)
    args = parser.parse_args(argv)
    try:
        if args.command == "fetch":
            fetch(custom=args.get_custom_messages)
        else:
            build(args.workspace, args.standalone, args.with_tests, args.rmw)
    except (BuildError, subprocess.CalledProcessError, OSError, ValueError, KeyError) as exc:
        print("Lyrical build failed: " + str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
