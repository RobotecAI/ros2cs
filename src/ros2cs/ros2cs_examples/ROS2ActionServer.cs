// Copyright 2019-2021 Robotec.ai
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
using ROS2;
using example_interfaces.action;

namespace Examples
{
  /// <summary> A simple action server class to illustrate Ros2cs </summary>
  public class ROS2ActionServer
  {
    public static IAction<Fibonacci_Goal, Fibonacci_Feedback, Fibonacci_Result> my_action_server;

    public static void Main(string[] args)
    {
      Console.WriteLine("ActionServer start")
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("action_server");
      my_action_server = node.CreateActionServer<Fibonacci_Goal, Fibonacci_Feedback, Fibonacci_Result>(
        "fibonacci",
        goal_callback
      )

      Ros2cs.Spin(node);
      Ros2cs.Shutdown();
    }

    public static bool goal_callback()
    {
      Console.WriteLine("Receiving new action goal...");
    }
  }
}
