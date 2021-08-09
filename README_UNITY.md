# Working with ros2cs and Unity game engine

## Deploying assemblies

For convenience, deploy scripts:
 - `deploy_unity_plugins.sh <TARGET_PLUGINS_DIR>` for Linux and
 - `deploy_unity_plugins.ps1 <TARGET_PLUGINS_DIR>` for Windows 

are attached in main `ros2cs` repo directory. `<TARGET_PLUGINS_DIR>` is the destination argument which should point to a proper directory inside `Unity` project.

Default Unity editor plugin directory is `Assets/ROS2/Plugins` located in your Unity3D project root directory (one with the `Assets` directory). Therefore for example in a new project you should call (assuming you set `PROJECT_ROOT`):
- `deploy_unity_plugins.ps1 %PROJECT_ROOT%\Assets\ROS2\Plugins` (Windows) or 
- `deploy_unity_plugins.sh ${PROJECT_ROOT}/Assets/ROS2/Plugins` (Linux).

Note that these scripts will add the `OS` and `architecture` parts of the final destination path automatically. They deploy libraries from `install/lib`, `install/bin` and `install/standalone` (optionally, if set) to two directories:
- `<TARGET_PLUGINS_DIR>` for managed (platform-independent) libraries, and 
- `<TARGET_PLUGINS_DIR>\<OS>\x86_64` for native (Windows or Linux) libraries.

## Standalone mode

If `ros2cs` is built with `STANDALONE_BUILD` option set to `1`, then nothing extra needs to be done. Otherwise, you have to source `ROS2` before launching any `ros2cs` application (either editor or application).

## Running the Editor

#### Windows

On Windows `ros2-for-unity` plugin takes care of the environment variable setup in runtime, so you don't need to worry about this step.

#### Linux

After deploying binaries, you need to use `start.py` script from the `ros2-for-unity` to run your Unity3D Editor or built App.
This is best done after running `colcon build`, sourcing your workspace and calling `ros2 run ros2-for-unity start editor`.

> This is due to the necessity for Unity3D to find all required assemblies. `dlopen` is used under the hood to dynamically
   load necessary libraries (including custom messages and rmw implementation).
   Note that unlike Windows `LoadLibrary`, `dlopen` uses `LD_LIBRARY_PATH` value from the start of the process execution and later modifications do not
   affect the search paths. Thus, it is necessary to change `LD_LIBRARY_PATH` before the process starts.

> Note that as an alternative, you can set the path manually in your system for the Editor, but consider that built App will have a different absolute path to plugins and can be moved and copied around, so this approach is not recommended.

### Building Unity3D application

When there are no errors in the Editor, you can proceed with an application build.
You can do this standard way through `Build->Build Settings...`.

## Running application 

### Windows

You can run your application normally by clicking it or executing from command line.

### Linux

On Linux, follow the [Editor instructions](#running-the-editor), but use `ros2 run ros2-for-unity start app --name:=<Your app name>` to run your application. 
This is necessary to ensure plugin visibility by setting `LD_LIBRARY_PATH`. 

## Full example (Windows)

Example for setting up `ros2cs` standalone with `Unity` editor on Windows (powershell with git). Let's assume working directory is `C:\dev` and we are using `ROS2 foxy`:

1. Install ros2 distribution, either Foxy or Galactic. We assume standard directory, e.g. `C:\dev\ros2_foxy`.
   You can find instructions [here (ros2 binary)](https://docs.ros.org/en/foxy/Installation/Windows-Install-Binary.html) and [here (development tools)](https://docs.ros.org/en/foxy/Installation/Windows-Development-Setup.html). 
2. Make sure you are running `Developer PowerShell for Visual Studio` (see **Building** section in `ros2cs` README.md).
3. Source ROS2
```
C:\dev\ros2_foxy\local_setup.ps1
```
4. Clone `ros2cs` repository.
5. Make sure you have necessary [prerequisites for ros2cs](README.md).
6. Get `ros2cs` required repos:
```
cd ros2cs
.\get_repos.ps1
```
7. Build `ros2cs` (default builds with standalone libraries):
```
.\build.ps1
```
8. Deploy `ros2cs` plugins to `Ros2 For Unity` asset directory:
```
.\deploy_unity_plugins.ps1 C:\dev\<UnityProjectPath>\Assets\ROS2\Plugins
```
9. Launch your Unity3D Project. The following command assumes that you have Unity3D installed in `C:\Program Files\Unity\Hub\Editor\2021.1.7f1\Editor\Unity.exe`:
```
& "C:\Program Files\Unity\Hub\Editor\2021.1.7f1\Editor\Unity.exe" -projectPath C:\dev\<UnityProjectPath>
```
10. You should be able to use `Ros2 For Unity` with new `ros2cs` plugins now. You can test if everything works through `Ros2 For Unity` test publisher.
> Note that after you build ros2cs plugins, you can use them on a machine that has no ros2 installation.
> You can simply copy your updated ROS2 Asset. 