# SPDX-License-Identifier: Apache-2.0
import importlib.util
from pathlib import Path
import os
import shutil
import subprocess
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("overlay_driver", ROOT / "tools/lyrical.py")
driver = importlib.util.module_from_spec(spec)
spec.loader.exec_module(driver)


class OverlayTests(unittest.TestCase):
    def test_overlay_typesupport_shadows_underlay(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            overlay, underlay = root / "install/lib", root / "ros/lib"
            overlay.mkdir(parents=True)
            underlay.mkdir(parents=True)
            name = "libstd_msgs__rosidl_typesupport_c.so"
            (overlay / name).write_bytes(b"new")
            (underlay / name).write_bytes(b"old")
            for stem in ("ros2cs_native", "rcl", "rcutils", "rmw_implementation", "rmw_fastrtps_cpp", "rcl_logging_spdlog"):
                (underlay / ("lib" + stem + ".so")).write_bytes(b"fixture")
            with patch.object(driver.platform, "system", return_value="Linux"):
                result = driver.runtime_seeds(root / "install", [overlay, underlay], "rmw_fastrtps_cpp")
            self.assertIn(overlay / name, result)
            self.assertNotIn(underlay / name, result)
            output = root / "out"
            output.mkdir()
            for source in result:
                driver.copy_unique(source, output)
            self.assertEqual((output / name).read_bytes(), b"new")

    def test_unrelated_conflicts_still_fail(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            a, b = root / "a", root / "b"
            a.mkdir()
            b.mkdir()
            (a / "same.dll").write_bytes(b"a")
            (b / "same.dll").write_bytes(b"b")
            with self.assertRaisesRegex(driver.BuildError, "outside search roots"):
                driver.prefer_runtime_libraries([a / "same.dll", b / "same.dll"], [a])

    def test_windows_case_insensitive_basename(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            a, b = root / "a", root / "b"
            a.mkdir()
            b.mkdir()
            (a / "TEST.dll").write_bytes(b"overlay")
            (b / "test.dll").write_bytes(b"underlay")
            with patch.object(driver.platform, "system", return_value="Windows"):
                result = driver.prefer_runtime_libraries([a / "TEST.dll", b / "test.dll"], [a, b])
            self.assertEqual(result, [a / "TEST.dll"])

    def test_replacement_transitive_dependencies_are_rescanned(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            overlay, underlay = root / "overlay", root / "underlay"
            overlay.mkdir()
            underlay.mkdir()
            for path in (overlay / "seed.dll", overlay / "dep.dll", overlay / "extra.dll", underlay / "dep.dll"):
                path.write_bytes(str(path).encode())
            scans = []
            def scan(args, **kwargs):
                config = (root / "scan/inputs.cmake").read_text()
                scans.append(config)
                dependencies = [underlay / "dep.dll"]
                if str(overlay / "dep.dll").replace("\\", "/") in config:
                    dependencies.append(overlay / "extra.dll")
                (root / "scan/resolved.txt").write_text("\n".join(map(str, dependencies)))
            with patch.object(driver, "require_tools"), patch.object(driver, "run", side_effect=scan):
                result = driver.dependency_closure([overlay / "seed.dll"], [overlay, underlay], root / "scan")
            self.assertIn(overlay / "dep.dll", result)
            self.assertIn(overlay / "extra.dll", result)
            self.assertNotIn(underlay / "dep.dll", result)
            self.assertEqual(len(scans), 3)


@unittest.skipUnless(os.name == "nt" and all(shutil.which(x) for x in ("cl", "dumpbin", "cmake")), "MSVC/PE tools not available")
class PeIntegrationTests(unittest.TestCase):
    def fixture(self, folder, value=42):
        folder.mkdir(parents=True)
        (folder / "dep.c").write_text("__declspec(dllexport) int fixture_dep(void) { return %d; }" % value)
        (folder / "parent.c").write_text("__declspec(dllimport) int fixture_dep(void); __declspec(dllexport) int fixture_parent(void) { return fixture_dep(); }")
        subprocess.run(["cl", "/nologo", "/LD", "/MT", "dep.c", "/link", "/OUT:fixture_dep.dll", "/IMPLIB:fixture_dep.lib"], cwd=folder, check=True, stderr=subprocess.STDOUT)
        subprocess.run(["cl", "/nologo", "/LD", "/MT", "parent.c", "fixture_dep.lib", "/link", "/OUT:fixture_parent.dll"], cwd=folder, check=True, stderr=subprocess.STDOUT)

    def test_pe_dependency_closure(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            libs = root / "native with spaces"
            self.fixture(libs)
            result = driver.dependency_closure([libs / "fixture_parent.dll"], [libs], root / "scan")
            self.assertIn(libs / "fixture_dep.dll", result)

    def test_pe_missing_dependency_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            libs = root / "libs"
            self.fixture(libs)
            (libs / "fixture_dep.dll").unlink()
            with self.assertRaises(subprocess.CalledProcessError):
                driver.dependency_closure([libs / "fixture_parent.dll"], [libs], root / "scan")

    def test_pe_overlay_is_selected(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            overlay, underlay = root / "overlay", root / "underlay"
            self.fixture(overlay, 43)
            self.fixture(underlay, 42)
            result = driver.dependency_closure([underlay / "fixture_parent.dll"], [overlay, underlay], root / "scan")
            self.assertIn(overlay / "fixture_parent.dll", result)
            self.assertIn(overlay / "fixture_dep.dll", result)
            self.assertNotIn(underlay / "fixture_dep.dll", result)


if __name__ == "__main__":
    unittest.main()
