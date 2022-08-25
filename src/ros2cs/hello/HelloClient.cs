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
using System.Threading;
using ROS2;
using hello_interfaces;
using example_interfaces;

namespace Hello
{
  /// <summary> A simple service client class to illustrate Ros2cs in action </summary>
  public class HelloClient
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("Hello Client start");
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("client");
      Client<hello_interfaces.srv.AddThreeInts_Request> my_client = node.CreateClient<hello_interfaces.srv.AddThreeInts_Request>("add_three_ints");

      hello_interfaces.srv.AddThreeInts_Request msg = new hello_interfaces.srv.AddThreeInts_Request();
      msg.A = 4;
      msg.B = 1;
      msg.C = 3;

      my_client.WaitForService(msg);

      IntPtr ptr;
      ptr = my_client.SendAndRecv(msg);
      int sum = (int)ptr;
      Console.WriteLine("Sum = " + sum);

      Console.WriteLine("Client shutdown");
      Ros2cs.Shutdown();
    }
  }
}
