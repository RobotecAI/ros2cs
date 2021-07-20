// Copyright 2021 Robotec.ai
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

using System;
using System.Collections.Generic;

namespace ROS2
{
  public enum LogLevel
  {
    DEBUG,
    INFO,
    WARNING,
    ERROR
  }

  /// <summary> A simple logging class for Ros2cs </summary>
  public class Ros2csLogger
  {
    private Ros2csLogger() { }
    private static Ros2csLogger _instance;

    public delegate void Callback(object message);

    private static Dictionary<LogLevel, String> LevelNames = new Dictionary<LogLevel, String>()
    {
      {LogLevel.DEBUG, "DEBUG"},
      {LogLevel.INFO, "INFO"},
      {LogLevel.WARNING, "WARNING"},
      {LogLevel.ERROR, "ERROR"},
    };

    public static LogLevel LogLevel { get; set; }

    private static Dictionary<LogLevel, Callback> LevelCallbacks = new Dictionary<LogLevel, Callback>()
    {
      {LogLevel.DEBUG, null},
      {LogLevel.INFO, null},
      {LogLevel.WARNING, null},
      {LogLevel.ERROR, null},
    };

    private static Dictionary<LogLevel, ConsoleColor> LevelColors = new Dictionary<LogLevel, ConsoleColor>()
    {
      {LogLevel.DEBUG, ConsoleColor.Green},
      {LogLevel.INFO, ConsoleColor.White},
      {LogLevel.WARNING, ConsoleColor.Yellow},
      {LogLevel.ERROR, ConsoleColor.Red},
    };

    /// <summary> Set a callback for an application layer logger </summary>
    /// <description> Can be useful to standardize logging between Ros2cs and
    /// an application (e. g. in Unity3D) which is using it </description>
    /// <param name="level"> Log level as in LogLevel enum </param>
    /// <param name="cb"> Callback (logging mechanism) to execute when logging </param>
    public static void setCallback(LogLevel level, Callback cb)
    {
      LevelCallbacks[level] = cb;
    }

    /// <summary> Acquire the singleton </summary>
    /// <description> Implements lazy construction </description>
    public static Ros2csLogger GetInstance()
    {
      if (_instance == null)
      {
        _instance = new Ros2csLogger();
      }
      return _instance;
    }

    /// <summary> Log a given message with a set level </summary>
    /// <param name="level"> Log level as in LogLevel enum </param>
    /// <param name="message"> Message to log </param>
    public void Log(LogLevel level, String message)
    {
      if (Ros2csLogger.LogLevel > level) return;

      ConsoleColor prevForeground = Console.ForegroundColor;
      Console.ForegroundColor = Ros2csLogger.LevelColors[level];
      if(Ros2csLogger.LevelCallbacks[level] != null)
      {
        Ros2csLogger.LevelCallbacks[level]("[ROS2CS] " + message);
      }
      Console.WriteLine(
          "[" +
          DateTime.Now.ToString("HH:mm:ss.ffffff") +
          "][" +
          Ros2csLogger.LevelNames[level] +
          "] " +
          message);
      Console.ForegroundColor = prevForeground;
    }

    public void LogInfo(String message)
    {
      Log(LogLevel.INFO, message);
    }

    public void LogWarning(String message)
    {
      Log(LogLevel.WARNING, message);
    }

    public void LogError(String message)
    {
      Log(LogLevel.ERROR, message);
    }

    public void LogDebug(String message)
    {
      Log(LogLevel.DEBUG, message);
    }
  }
}
