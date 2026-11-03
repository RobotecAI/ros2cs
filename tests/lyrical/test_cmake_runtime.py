# SPDX-License-Identifier: Apache-2.0
from pathlib import Path
import os
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]


@unittest.skipUnless(shutil.which("cmake") and (shutil.which("cl") or shutil.which("cc")), "CMake/C compiler not available")
class GeneratedRuntimeLinkTests(unittest.TestCase):
    def configure(self, missing=False):
        temporary = tempfile.TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        root = Path(temporary.name)
        (root / "include").mkdir()
        (root / "include/fixture.h").write_text("#define FIXTURE_VALUE 42\n")
        (root / "probe.c").write_text("#include <fixture.h>\nint probe(void) { return FIXTURE_VALUE; }\n")
        helper = ROOT / "src/ros2cs/rosidl_generator_cs/cmake/rosidl_generator_cs_link_runtime.cmake"
        source = """cmake_minimum_required(VERSION 3.21)
project(runtime_link_fixture C)
foreach(name rosidl_runtime_c rosidl_typesupport_c rosidl_typesupport_interface)
  add_library(${name}::${name} INTERFACE IMPORTED)
endforeach()
add_library(example_msgs::example_msgs__rosidl_generator_c INTERFACE IMPORTED)
set_property(TARGET example_msgs::example_msgs__rosidl_generator_c PROPERTY
  INTERFACE_INCLUDE_DIRECTORIES "${CMAKE_CURRENT_SOURCE_DIR}/include")
add_library(probe STATIC probe.c)
include("@HELPER@")
rosidl_generator_cs_link_runtime(probe @PACKAGE@)
""".replace("@HELPER@", helper.as_posix()).replace("@PACKAGE@", "missing_msgs" if missing else "example_msgs")
        (root / "CMakeLists.txt").write_text(source)
        args = ["cmake", "-S", str(root), "-B", str(root / "build")]
        if os.name == "nt" and not os.environ.get("CMAKE_GENERATOR") and shutil.which("nmake"):
            args += ["-G", "NMake Makefiles"]
        result = subprocess.run(args, text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        return root, result

    def test_generated_dependency_propagates_headers_and_builds(self):
        root, result = self.configure()
        self.assertEqual(result.returncode, 0, result.stdout)
        built = subprocess.run(["cmake", "--build", str(root / "build"), "--config", "Release"], text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        self.assertEqual(built.returncode, 0, built.stdout)

    def test_missing_generated_runtime_target_fails_clearly(self):
        _, result = self.configure(missing=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Missing generated C runtime target", result.stdout)
