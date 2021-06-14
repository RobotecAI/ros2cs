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

    public class Ros2csLogger
    {
        private Ros2csLogger() { }
        private static Ros2csLogger _instance;

        public delegate void Callback(object message);

        public static Dictionary<LogLevel, String> LevelNames = new Dictionary<LogLevel, String>()
        {
            {LogLevel.DEBUG, "DEBUG"},
            {LogLevel.INFO, "INFO"},
            {LogLevel.WARNING, "WARNING"},
            {LogLevel.ERROR, "ERROR"},
        };

        public static LogLevel LogLevel { get; set; }

        public static Dictionary<LogLevel, Callback> LevelCallbacks = new Dictionary<LogLevel, Callback>()
        {
            {LogLevel.DEBUG, null},
            {LogLevel.INFO, null},
            {LogLevel.WARNING, null},
            {LogLevel.ERROR, null},
        };

        public static Dictionary<LogLevel, ConsoleColor> LevelColors = new Dictionary<LogLevel, ConsoleColor>()
        {
            {LogLevel.DEBUG, ConsoleColor.Green},
            {LogLevel.INFO, ConsoleColor.White},
            {LogLevel.WARNING, ConsoleColor.Yellow},
            {LogLevel.ERROR, ConsoleColor.Red},
        };

        public static void setCallback(LogLevel level, Callback cb)
        {
            LevelCallbacks[level] = cb;
        }

        public static Ros2csLogger GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Ros2csLogger();
            }
            return _instance;
        }

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