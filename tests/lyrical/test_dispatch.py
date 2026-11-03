"""The managed loader needs the generic C dispatch binding for either DDS."""
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]

@unittest.skipUnless(shutil.which('cmake'), 'CMake unavailable')
class DispatchTests(unittest.TestCase):
    def test_lyrical_produces_dispatch_binding_not_concrete_dds_names(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            for pkg in ('rmw_implementation_cmake', 'rosidl_typesupport_c'):
                folder = root / pkg
                folder.mkdir()
                (folder / (pkg + 'Config.cmake')).write_text('')
            helper = ROOT / 'src/ros2cs/rosidl_generator_cs/cmake/rosidl_generator_cs_get_typesupports.cmake'
            script = root / 'check.cmake'
            script.write_text('set(CMAKE_PREFIX_PATH "' + root.as_posix() + '")\n'
                              'include("' + helper.as_posix() + '")\n'
                              'rosidl_generator_cs_get_typesupports(selected)\n'
                              'if(NOT selected STREQUAL "rosidl_typesupport_c")\n'
                              ' message(FATAL_ERROR "Expected one generic dispatch binding: ${selected}")\n'
                              'endif()\n')
            result = subprocess.run(['cmake', '-P', str(script)], env={**os.environ, 'ROS_DISTRO': 'lyrical'}, capture_output=True, text=True)
            self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
