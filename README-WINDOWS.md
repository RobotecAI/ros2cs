# ROS2CS - Windows 10

## Building

### Prerequisites

*  ROS2 installed on the system (additionally you should go to [Building ROS2 section](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html) and check if all `pip` [Install dependencies](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html#install-dependencies) and [Developer tools](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html#install-developer-tools) are installed)
*  vcstool package - [see here](https://github.com/dirk-thomas/vcstool)
*  .NET 5.0 sdk - [see here](https://dotnet.microsoft.com/download/dotnet/5.0)
*  For tests only: xUnit testing framework - [see here](https://xunit.net/)

### Important notices

- Windows [path length is limited to 260 characters](https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation) by default. A good solution is to modify your `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem` registry key `LongPathsEnabled` to 1. This way you will avoid path length issues. Alternatively, you need to clone your repo to `C:\dev` into `r2cs` folder or a similar shallow path to avoid this issue during build. **Cloning into longer path will cause compilation errors!**

- For building and running a Visual Studio preconfigured powershell terminal must be used. Standard powershell prompt might not be configured properly to be used with MSVC compiler and Windows SDKs.  You should have Visual Studio already installed (ROS2 dependency) and you can find shortcut for `Developer PowerShell for VS` here: `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Visual Studio 2019\Visual Studio Tools`.

- A powershell terminal with administrator privileges is required for **Windows** and **ros2 galactic**. This is because python packages installation requires a privilage for creating symlinks. More about this issue: [github issue](https://github.com/ament/ament_cmake/issues/350).

- There is a bug with hardcoded include exports in some **ros2 galactic** packages on **Windows**. Easiest workaround is to create a `C:\ci\ws\install\include` directory in your system. More about this bug and proposed workarounds: [github issue](https://github.com/ros2/rclcpp/issues/1688#issuecomment-858467147).

- Sometimes it is required to set NuGet package feed to nuget.org: `dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json` in order to resolve some missing packages for `ros2cs` project.

### Steps

- Clone this project.
- Source your ROS2 installation (`C:\dev\ros2_foxy\local_setup.ps1`)
- Navigate to the top project folder and pull required repositories (`get_repos.ps1`)
  - You can run script with `--get-custom-messages` argument to fetch extra messages from `custom_messages.repos` file.
  - It will use `vcstool` to download required ROS2 packages. By default, this will get repositories as set in `${ROS_DISTRO}`.
- Build package (`build.ps1`)
  - It invokes `colcon_build` with `--merge-install` argument to simplify libraries installat
  - You can build tests by adding `--with-tests` argument
- To test your build please check main readme [Testing section](README.md#testing)

### Standalone version (Windows)

By default, Windows build process generates standalone libraries in `install/standalone` directory.
You can disable this feature by setting CMake option `STANDALONE_BUILD` to `OFF`.

To run standalone application you must deploy it with libraries from both `install/bin` and `install/standalone`.

To run examples with standalone build you should modify `PATH`  environment variable so your executable will find all the required libraries (if executable lies in a different directory than libraries).
Additionally all libraries need to be in a visible path, since they are loaded dynamically at runtime.

## Troubleshooting

- Tests are not working ('charmap' codec can't decode byte) on Windows

Problem may occur on non english version of Windows. This error is caused by impossibility in decoding `dotnet` output by ament tools.

**Fix**: Change your `dotnet` output to english by temporarily renaming your localization directory (`pl` to `pl.bak`, `fr` to `fr.bak` etc.) in your `dotnet` sdk directory.

**If no solution of your problem is present in the section above, please make sure to check out `ROS2 For Unity` [Troubleshooting section](https://github.com/RobotecAI/ros2-for-unity/blob/master/README-WINDOWS.md#build-troubleshooting)**