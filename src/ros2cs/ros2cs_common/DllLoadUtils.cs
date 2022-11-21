// Copyright 2019-2021 Robotec.ai
// Copyright 2016-2018 Esteve Fernandez <esteve@apache.org>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Based on http://dimitry-i.blogspot.com.es/2013/01/mononet-how-to-dynamically-load-native.html

using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace ROS2
{
  public class GlobalVariables {
    public static bool preloadLibrary = false;
    public static string preloadLibraryName = "";
    public static string absolutePath = "";
  }

  public enum Platform {
    Unix,
    MacOSX,
    WindowsDesktop,
    UWP,
    Unknown
  }

  public class DllLoadUtilsFactory {
    [DllImport ("api-ms-win-core-libraryloader-l2-1-0.dll", EntryPoint = "LoadPackagedLibrary", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr LoadPackagedLibrary ([MarshalAs (UnmanagedType.LPWStr)] string fileName, int reserved = 0);

    [DllImport ("api-ms-win-core-libraryloader-l1-2-0.dll", EntryPoint = "FreeLibrary", SetLastError = true, ExactSpelling = true)]
    private static extern int FreeLibraryUWP (IntPtr handle);

    [DllImport ("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary (string fileName);

    [DllImport ("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
    private static extern int FreeLibraryDesktop (IntPtr handle);

    [DllImport ("libdl.so.2", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen_unix (String fileName, int flags);

    [DllImport ("libdl.so.2", EntryPoint = "dlclose")]
    private static extern int dlclose_unix (IntPtr handle);

    [DllImport ("libdl.dylib", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen_macosx (String fileName, int flags);

    [DllImport ("libdl.dylib", EntryPoint = "dlclose")]
    private static extern int dlclose_macosx (IntPtr handle);

    const int RTLD_NOW = 2;

    public static DllLoadUtils GetDllLoadUtils () {
      switch (CheckPlatform ()) {
        case Platform.Unix:
          return new DllLoadUtilsUnix ();
        case Platform.MacOSX:
          return new DllLoadUtilsMacOSX ();
        case Platform.WindowsDesktop:
          return new DllLoadUtilsWindowsDesktop ();
        case Platform.UWP:
          return new DllLoadUtilsUWP ();
        case Platform.Unknown:
        default:
          throw new UnknownPlatformError ();
      }
    }

    private static bool IsUWP () {
      try {
        IntPtr ptr = LoadPackagedLibrary ("api-ms-win-core-libraryloader-l2-1-0.dll");
        FreeLibraryUWP (ptr);
        return true;
      } catch (TypeLoadException) {
        return false;
      }
    }

    private static bool IsWindowsDesktop () {
      try {
        IntPtr ptr = LoadLibrary ("kernel32.dll");
        FreeLibraryDesktop (ptr);
        return true;
      } catch (TypeLoadException) {
        return false;
      }
    }

    private static bool IsUnix () {
      try {
        IntPtr ptr = dlopen_unix ("libdl.so.2", RTLD_NOW);
        dlclose_unix (ptr);
        return true;
      } catch (TypeLoadException) {
        return false;
      }
    }

    private static bool IsMacOSX () {
      try {
        IntPtr ptr = dlopen_macosx ("libdl.dylib", RTLD_NOW);
        dlclose_macosx (ptr);
        return true;
      } catch (TypeLoadException) {
        return false;
      }
    }

    private static Platform CheckPlatform () {
          if (IsMacOSX())
          {
              return Platform.MacOSX;
          }
          else if (IsUnix())
          {
              return Platform.Unix;
          }
          else if (IsWindowsDesktop())
          {
              return Platform.WindowsDesktop;
          }
          else if (IsUWP())
          {
              return Platform.UWP;
          }
          else
          {
              return Platform.Unknown;
          }
    }
  }

  public interface DllLoadUtils {
    IntPtr LoadLibrary (string fileName);
    IntPtr LoadLibraryNoSuffix (string fileName);
    void FreeLibrary (IntPtr handle);
    IntPtr GetProcAddress (IntPtr dllHandle, string name);
  }

  public class DllLoadUtilsUWP : DllLoadUtils {

    [DllImport ("api-ms-win-core-libraryloader-l2-1-0.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr LoadPackagedLibrary ([MarshalAs (UnmanagedType.LPWStr)] string fileName, int reserved = 0);

    [DllImport ("api-ms-win-core-libraryloader-l1-2-0.dll", SetLastError = true, ExactSpelling = true)]
    private static extern int FreeLibrary (IntPtr handle);

    [DllImport ("api-ms-win-core-libraryloader-l1-2-0.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr GetProcAddress (IntPtr handle, string procedureName);

    void DllLoadUtils.FreeLibrary (IntPtr handle) {
      FreeLibrary (handle);
    }

    IntPtr DllLoadUtils.GetProcAddress (IntPtr dllHandle, string name) {
      return GetProcAddress (dllHandle, name);
    }

    IntPtr DllLoadUtils.LoadLibrary (string fileName) {
      string libraryName = fileName + "_native.dll";
      IntPtr ptr = LoadPackagedLibrary (libraryName);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }

    IntPtr DllLoadUtils.LoadLibraryNoSuffix (string fileName) {
      string libraryName = fileName + ".dll";
      IntPtr ptr = LoadPackagedLibrary (libraryName);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }
  }

  public class DllLoadUtilsWindowsDesktop : DllLoadUtils {

    [DllImport ("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary (string fileName);

    [DllImport ("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern int FreeLibrary (IntPtr handle);

    [DllImport ("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr GetProcAddress (IntPtr handle, string procedureName);

    void DllLoadUtils.FreeLibrary (IntPtr handle) {
      FreeLibrary (handle);
    }

    IntPtr DllLoadUtils.GetProcAddress (IntPtr dllHandle, string name) {
      return GetProcAddress (dllHandle, name);
    }

    IntPtr DllLoadUtils.LoadLibrary (string fileName) {
      string libraryName = fileName + "_native.dll";
      IntPtr ptr = LoadLibrary (libraryName);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }

    IntPtr DllLoadUtils.LoadLibraryNoSuffix (string fileName) {
      string libraryName = fileName + ".dll";
      IntPtr ptr = LoadLibrary (libraryName);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }
  }

  internal class DllLoadUtilsUnix : DllLoadUtils {

    [DllImport ("libdl.so.2", ExactSpelling = true)]
    private static extern IntPtr dlopen (String fileName, int flags);

    [DllImport ("libdl.so.2", ExactSpelling = true)]
    private static extern IntPtr dlsym (IntPtr handle, String symbol);

    [DllImport ("libdl.so.2", ExactSpelling = true)]
    private static extern int dlclose (IntPtr handle);

    [DllImport ("libdl.so.2", ExactSpelling = true)]
    private static extern IntPtr dlerror ();

    const int RTLD_NOW = 0x00002;
    const int RTLD_DEEPBIND = 0x00008;

    //TODO (adamdbrw) Somewhat hacky solution to open (and dereference) the problematic library
    //that otherwise causes crashes in Unity Editor.
    private static bool libPreloaded = false;
    void CheckPreloadLibraries()
    {
        if (libPreloaded || GlobalVariables.preloadLibraryName == "")
            return;
        Ros2csLogger.GetInstance().LogDebug("Preloading " + GlobalVariables.preloadLibraryName);
        IntPtr libPtr = Load(GlobalVariables.preloadLibraryName);

        Ros2csLogger.GetInstance().LogDebug("Preloading " + GlobalVariables.preloadLibraryName + " successful.");

        libPreloaded = true;
    }

    public void FreeLibrary (IntPtr handle) {
      dlclose (handle);
    }

    public IntPtr GetProcAddress (IntPtr dllHandle, string name) {
      // clear previous errors if any
      dlerror ();
      var res = dlsym (dllHandle, name);
      var errPtr = dlerror ();
      if (errPtr != IntPtr.Zero) {
        throw new Exception ("dlsym: " + Marshal.PtrToStringAnsi (errPtr));
      }
      return res;
    }

    private IntPtr Load(string libraryFileName) {
      string libraryPath = GlobalVariables.absolutePath + libraryFileName;
      string dlopenSearchString = libraryPath;
      Ros2csLogger.GetInstance().LogDebug("Loading lib: " + dlopenSearchString);
      IntPtr ptr = dlopen(dlopenSearchString, RTLD_NOW);
      if (ptr == IntPtr.Zero) {
        if (!String.IsNullOrEmpty(GlobalVariables.absolutePath)) {
          // Fallback - look for library in default paths
          var errPtr = dlerror ();
          Ros2csLogger.GetInstance().LogDebug("Could not find " + dlopenSearchString + ": " + Marshal.PtrToStringAnsi (errPtr) + ". Fallback to " + libraryFileName);
          dlopenSearchString = libraryFileName;
          ptr = dlopen(dlopenSearchString, RTLD_NOW);
        }
      }      
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError(dlopenSearchString);
      }
      Ros2csLogger.GetInstance().LogDebug("Loaded library: " + dlopenSearchString);
      return ptr;
    }

    private IntPtr LoadLibraryByName(string libraryFileName) {
      if (GlobalVariables.preloadLibrary)
        CheckPreloadLibraries();
      return Load(libraryFileName);
    }

    public IntPtr LoadLibrary(string fileName) {
      string libraryName = "lib" + fileName + "_native.so";
      return LoadLibraryByName(libraryName);
    }

    public IntPtr LoadLibraryNoSuffix(string fileName) {
      string libraryName = "lib" + fileName + ".so";
      return LoadLibraryByName(libraryName);
    }
  }

  internal class DllLoadUtilsMacOSX : DllLoadUtils {

    [DllImport ("libdl.dylib", ExactSpelling = true)]
    private static extern IntPtr dlopen (String fileName, int flags);

    [DllImport ("libdl.dylib", ExactSpelling = true)]
    private static extern IntPtr dlsym (IntPtr handle, String symbol);

    [DllImport ("libdl.dylib", ExactSpelling = true)]
    private static extern int dlclose (IntPtr handle);

    [DllImport ("libdl.dylib", ExactSpelling = true)]
    private static extern IntPtr dlerror ();

    const int RTLD_NOW = 2;

    public void FreeLibrary (IntPtr handle) {
      dlclose (handle);
    }

    public IntPtr GetProcAddress (IntPtr dllHandle, string name) {
      // clear previous errors if any
      dlerror ();
      var res = dlsym (dllHandle, name);
      var errPtr = dlerror ();
      if (errPtr != IntPtr.Zero) {
        throw new Exception ("dlsym: " + Marshal.PtrToStringAnsi (errPtr));
      }
      return res;
    }

    public IntPtr LoadLibrary (string fileName) {
      string libraryName = "lib" + fileName + "_native.dylib";
      IntPtr ptr = dlopen (libraryName, RTLD_NOW);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }

    public IntPtr LoadLibraryNoSuffix (string fileName) {
      string libraryName = "lib" + fileName + ".dylib";
      IntPtr ptr = dlopen (libraryName, RTLD_NOW);
      if (ptr == IntPtr.Zero) {
        throw new UnsatisfiedLinkError (libraryName);
      }
      return ptr;
    }
  }
}
