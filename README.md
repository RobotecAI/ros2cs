Ros2cs
=============

A C# .NET library for ROS2, including C# implementation of rcl APIs, message generation, tests and examples.

Ros2cs is also an independent part of [Ros2 For Unity](https://github.com/RobotecAI/ros2-for-unity), which enables high-performance communication between simulation and ROS2 robot packages. Follow instructions there instead if you are intending to use ros2cs with Unity3D.

### Features

- A set of core abstractions such as Node, Publisher, Subscription, QoS, Clock
- Comes with support for all standard ros2 messages
- Custom messages can be easily generated from unmodified ROS2 packages
- A logger that can be hooked to your application callbacks (e.g. in Unity3D)

### Platforms

Supported OSes:
- Ubuntu 22.04 (bash)
- Ubuntu 20.04 (bash)
- Windows 10 (powershell)

Supported ROS2 distributions:
- Foxy
- Galactic
- Humble

### Flavours

`ros2cs` libraries can be built in two flavors:
- _standalone_ (no ROS2 installation required on the target machine, e.g., your Unity3D simulation server). All required dependencies are installed and can be used e.g., as a complete set of Unity3D plugins.
- _overlay_ (assuming existing (supported) ROS2 installation on the target machine). Only ros2cs libraries and generated messages are installed.

## Building

### Generating custom messages

After cloning the project and importing .repos, you can simply put your message package next to other packages in the `src/ros2` sub-folder. Then, build your project, and you have all messages generated. You can also modify and use the `custom_message.repos` template to automate the process with the `get_repos` script.

### Build instructions

Please follow the  OS-specific instructions for your build:

- [Ubuntu 20.04 Instructions](README-UBUNTU.md)
- [Windows 10 Instructions](README-WINDOWS.md)

## Testing

Make sure your NuGet repositories can resolve `xUnit` dependency. You can call `dotnet nuget list source` to see your current sources for NuGet packages. Please note that `Microsoft Visual Studio Offline Packages` are usually insufficient. You can fix it by adding `nuget.org` repository: `dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json`.

- Make sure you built tests ( OS-specific build script with `--with-tests` flag).
- Run OS-specific test script:
    - ubuntu:
    ```bash
    ./test.sh
    ```
    - windows:
    ```powershell
    test.sp1
    ```
- Run a manual test with basic listener/publisher examples (you have to source your ROS2 first):
    - ubuntu
    ```bash
    ros2 run ros2cs_examples ros2cs_talker
    ros2 run ros2cs_examples ros2cs_listener
    ```
    - windows
    ```
    ros2 run ros2cs_examples ros2cs_talker.exe
    ros2 run ros2cs_examples ros2cs_listener.exe
    ```
- Run a manual performance test (you have to source your ROS2 first):
    - ubuntu
    ```bash
    ros2 run ros2cs_examples ros2cs_performance_talker
    ros2 run ros2cs_examples ros2cs_performance_listener
    ```
    - windows
    ```
    ros2 run ros2cs_examples ros2cs_performance_talker.exe
    ros2 run ros2cs_examples ros2cs_performance_listener.exe
    ```

## Acknowledgements

The project started as a fork of [ros2_dotnet](https://github.com/ros2-dotnet/ros2_dotnet) but moved away from its root through new features and design choices. Nevertheless, ros2cs is built on foundation of open-source efforts of Esteve Fernandez (esteve), Lennart Nachtigall (firesurfer), Samuel Lindgren (samiamlabs) and other contributors to ros2_dotnet project.

Open-source release of ros2cs was made possible through cooperation with [Tier IV](https://tier4.jp). Thanks to encouragement, support and requirements driven by Tier IV the project was significantly improved in terms of portability, stability, core structure and user-friendliness.
