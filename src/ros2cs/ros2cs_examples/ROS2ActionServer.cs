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
using action_msgs.srv;
using ROS2;
using example_interfaces.action;

namespace Examples
{
  /// <summary> A simple action server class to illustrate Ros2cs </summary>
  public class ROS2ActionServer
  {
    public static IActionServer<
      Fibonacci_SendGoal_Request,
      Fibonacci_SendGoal_Response,
      Fibonacci_Feedback,
      Fibonacci_GetResult_Request,
      Fibonacci_GetResult_Response
    > my_action_server;

    public static void Main(string[] args)
    {
      Console.WriteLine("ActionServer start");
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("action_server");
      my_action_server = node.CreateActionServer<
        Fibonacci_SendGoal_Request,
        Fibonacci_SendGoal_Response,
        Fibonacci_Feedback,
        Fibonacci_GetResult_Request,
        Fibonacci_GetResult_Response>(
        "fibonacci",
        handle_goal,
        handle_cancel,
        handle_accepted
      );

      Ros2cs.Spin(node);
      Ros2cs.Shutdown();
    }

    /// <summary>
    /// Callback on receiving a goal - must return quickly!
    /// </summary>
    public static ActionGoalResponse handle_goal(Fibonacci_SendGoal_Request request)
    {
      Console.WriteLine("Receiving new action goal...");

      return ActionGoalResponse.ACCEPT_AND_EXECUTE;
    }

    /// <summary>
    /// Callback on receiving a cancel request - must return quickly!
    /// </summary>
    public static CancelGoal_Response handle_cancel(CancelGoal_Request request)
    {
      CancelGoal_Response response = new CancelGoal_Response();
      return response;
    }

    public static void handle_accepted(Fibonacci_SendGoal_Request request)
    {
      return;
    }

    /// <summary>
    /// Actual action
    ///
    /// This function should be called from a different thread and does not need to return any time soon.
    /// </summary>
    public static void execute()
    {
      return;
    }
  }
}
