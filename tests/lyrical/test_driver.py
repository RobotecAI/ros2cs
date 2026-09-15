# SPDX-License-Identifier: Apache-2.0
import importlib.util
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("cs_driver", ROOT / "tools/lyrical.py")
driver = importlib.util.module_from_spec(spec)
spec.loader.exec_module(driver)


class EnvironmentTests(unittest.TestCase):
    def test_build_and_test_use_merged_layout_and_explicit_generator(self):
        with tempfile.TemporaryDirectory() as temp:
            workspace = Path(temp)
            (workspace / "src").mkdir()
            (workspace / "install/lib/dotnet").mkdir(parents=True)
            for name in ("ros2cs_common.dll", "ros2cs_core.dll"):
                (workspace / "install/lib/dotnet" / name).touch()
            # A profile stamp makes these fixture install files valid inputs.
            profile = {"distro": "lyrical", "platform": "Windows", "architecture": "x86_64",
                       "rmw": "rmw_fastrtps_cpp", "standalone": False,
                       "configuration": "Release", "cmake_generator": "Ninja"}
            driver.write_json(workspace / "build/lyrical-build-profile.json", profile)
            with patch.dict(os.environ, {}, clear=True), patch.object(driver.platform, "system", return_value="Windows"), patch.object(driver, "check_environment", return_value="rmw_fastrtps_cpp"), patch.object(driver, "full_checkout"), patch.object(driver, "require_tools"), patch.object(driver, "find_library"), patch.object(driver, "run") as run:
                driver.build(workspace, with_tests=True, cmake_generator="Ninja")
                commands = [call.args[0] for call in run.call_args_list]
                self.assertIn("--merge-install", commands[0])
                self.assertEqual(commands[0][-2:], ["-G", "Ninja"])
                self.assertIn("--merge-install", commands[1])
                self.assertEqual(commands[2], ["colcon", "test-result", "--verbose"])
                self.assertEqual(os.environ["PYTHONUTF8"], "1")

    def test_wrong_distro_rejected(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "jazzy"}, clear=True):
            with self.assertRaises(driver.BuildError):
                driver.check_environment()

    def test_default_fastdds(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "lyrical"}, clear=True), patch.object(driver.platform, "machine", return_value="x86_64"), patch.object(driver.platform, "system", return_value="Linux"):
            self.assertEqual(driver.check_environment(), "rmw_fastrtps_cpp")

    def test_cyclone_explicit_selection(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "lyrical"}, clear=True), patch.object(driver.platform, "machine", return_value="AMD64"), patch.object(driver.platform, "system", return_value="Windows"):
            self.assertEqual(driver.check_environment("rmw_cyclonedds_cpp"), "rmw_cyclonedds_cpp")

    def test_unimplemented_rmw_rejected(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "lyrical"}, clear=True):
            with self.assertRaises(driver.BuildError):
                driver.check_environment("rmw_zenoh_cpp")

    def test_arm_rejected(self):
        with patch.dict(os.environ, {"ROS_DISTRO": "lyrical"}, clear=True), patch.object(driver.platform, "machine", return_value="aarch64"):
            with self.assertRaises(driver.BuildError):
                driver.check_environment()

    def test_subset_is_not_misrepresented_as_full_checkout(self):
        with tempfile.TemporaryDirectory() as temp:
            with self.assertRaisesRegex(driver.BuildError, "not a full"):
                driver.full_checkout(Path(temp))

    def test_colcon_failure_propagates(self):
        with tempfile.TemporaryDirectory() as temp:
            workspace = Path(temp)
            (workspace / "src").mkdir()
            with patch.object(driver, "check_environment", return_value="rmw_fastrtps_cpp"), patch.object(driver, "full_checkout"), patch.object(driver, "require_tools"), patch.object(driver, "run", side_effect=subprocess.CalledProcessError(7, ["colcon"])):
                with self.assertRaises(subprocess.CalledProcessError):
                    driver.build(workspace)
            self.assertFalse((workspace / "install/lyrical-build.json").exists())

    def test_unstamped_cache_is_not_reused(self):
        with tempfile.TemporaryDirectory() as temp:
            workspace = Path(temp)
            (workspace / "src").mkdir()
            (workspace / "build").mkdir()
            (workspace / "build/old.txt").write_text("jazzy")
            with patch.object(driver, "check_environment", return_value="rmw_fastrtps_cpp"), patch.object(driver, "full_checkout"), patch.object(driver, "require_tools"):
                with self.assertRaisesRegex(driver.BuildError, "Unstamped"):
                    driver.build(workspace)
            self.assertTrue((workspace / "build/old.txt").exists())

    def test_existing_dependencies_are_not_overwritten(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "src/ros2").mkdir(parents=True)
            (root / "src/ros2/keep.txt").write_text("keep")
            (root / "ros2_lyrical.repos").write_text("repositories: {}")
            with patch.object(driver, "check_environment"), patch.object(driver, "full_checkout"), patch.object(driver, "require_tools"), patch.object(driver, "run") as run:
                with self.assertRaises(driver.BuildError):
                    driver.fetch(root)
                run.assert_not_called()
            self.assertEqual((root / "src/ros2/keep.txt").read_text(), "keep")


class FileTests(unittest.TestCase):
    def test_linux_so_filter_and_python_exclusion(self):
        with tempfile.TemporaryDirectory() as temp:
            folder = Path(temp)
            for name in ("liba.so", "liba.so.1.2", "liba_python.so", "a.a", "a.dll", "a.pdb"):
                (folder / name).write_text("x")
            self.assertEqual([p.name for p in driver.native_libraries(folder, False)], ["liba.so", "liba.so.1.2"])

    def test_windows_dll_filter(self):
        with tempfile.TemporaryDirectory() as temp:
            folder = Path(temp)
            for name in ("rcl.dll", "message_py.dll", "message_python.dll", "ros2cs.lib"):
                (folder / name).write_text("x")
            self.assertEqual([p.name for p in driver.native_libraries(folder, True)], ["rcl.dll"])

    def test_missing_runtime_is_error(self):
        with self.assertRaisesRegex(driver.BuildError, "Required runtime"):
            driver.find_library("unavailable", [])

    def test_collision_identical_allowed_different_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            for name in ("a", "b", "out"):
                (root / name).mkdir()
            a, b = root / "a/libx.so", root / "b/libx.so"
            a.write_bytes(b"same")
            b.write_bytes(b"same")
            driver.copy_unique(a, root / "out")
            driver.copy_unique(b, root / "out")
            b.write_bytes(b"different")
            with self.assertRaises(driver.BuildError):
                driver.copy_unique(b, root / "out")

    def test_alias_is_portable_real_file(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "out").mkdir()
            real = root / "libx.so.2.1"
            real.write_bytes(b"lib")
            alias = root / "libx.so.2"
            alias.symlink_to(real.name)
            target = driver.copy_unique(alias, root / "out")
            self.assertFalse(target.is_symlink())
            self.assertEqual(target.read_bytes(), b"lib")

    def test_cmake_path_injection_rejected(self):
        with self.assertRaises(driver.BuildError):
            driver.cmake_quote("a;bad")
        self.assertEqual(driver.cmake_quote("a b"), "[===[a b]===]")

    def test_json_atomic_write(self):
        with tempfile.TemporaryDirectory() as temp:
            output = Path(temp) / "nested/test.json"
            driver.write_json(output, {"distro": "lyrical"})
            self.assertIn("lyrical", output.read_text())
            self.assertFalse(output.with_suffix(".json.tmp").exists())

    def test_manifest_and_generator_import(self):
        text = (ROOT / "ros2_lyrical.repos").read_text()
        self.assertNotIn("version: jazzy", text)
        self.assertEqual(text.count("version: lyrical"), 9)
        path = ROOT / "src/ros2cs/rosidl_generator_cs/rosidl_generator_cs/generate_cs_impl.py"
        code = path.read_text()
        self.assertIn("from rosidl_pycommon import generate_files", code)
        self.assertIn("if exc.name != 'rosidl_pycommon'", code)
        compile(code, str(path), "exec")


@unittest.skipUnless(shutil.which("gcc") and shutil.which("cmake") and shutil.which("objdump") and os.name != "nt", "ELF fixture tools not available")
class ElfIntegrationTests(unittest.TestCase):
    def make_fixture(self, root):
        libs = root / "native with spaces"
        libs.mkdir()
        (libs / "dep.c").write_text("int fixture_dep(void) { return 42; }\n")
        (libs / "parent.c").write_text("extern int fixture_dep(void); int fixture_parent(void) { return fixture_dep(); }\n")
        subprocess.run(["gcc", "-shared", "-fPIC", "dep.c", "-Wl,-soname,libfixture_dep.so.2", "-o", "libfixture_dep.so.2.1"], cwd=libs, check=True)
        (libs / "libfixture_dep.so.2").symlink_to("libfixture_dep.so.2.1")
        (libs / "libfixture_dep.so").symlink_to("libfixture_dep.so.2")
        subprocess.run(["gcc", "-shared", "-fPIC", "parent.c", "-L.", "-lfixture_dep", "-Wl,-rpath,$ORIGIN", "-o", "libfixture_parent.so"], cwd=libs, check=True)
        return libs

    def test_real_elf_transitive_dependency_and_soname(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            libs = self.make_fixture(root)
            found = driver.dependency_closure([libs / "libfixture_parent.so"], [libs], root / "scan")
            self.assertTrue(any(p.name == "libfixture_dep.so.2" for p in found), found)
            self.assertFalse(any(p.name.startswith("libc.so") for p in found))

    def test_real_elf_overlay_shadows_rpath_underlay(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / "overlay").mkdir()
            (root / "underlay").mkdir()
            overlay = self.make_fixture(root / "overlay")
            underlay = self.make_fixture(root / "underlay")
            parent = underlay / "libunderlay_consumer.so"
            (underlay / "libfixture_parent.so").rename(parent)
            found = driver.dependency_closure(
                [parent, overlay / "libfixture_dep.so.2"],
                [overlay, underlay], root / "scan")
            self.assertIn(overlay / "libfixture_dep.so.2", found)
            self.assertNotIn(underlay / "libfixture_dep.so.2", found)

    def test_real_elf_missing_dependency_is_fatal(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            libs = self.make_fixture(root)
            (libs / "libfixture_dep.so.2").unlink()
            (libs / "libfixture_dep.so.2.1").unlink()
            with self.assertRaises(subprocess.CalledProcessError):
                driver.dependency_closure([libs / "libfixture_parent.so"], [libs], root / "scan")


if __name__ == "__main__":
    unittest.main()
