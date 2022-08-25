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
using System.Collections.Generic;
using ROS2;

namespace Hello
{
  /// <summary> A simple service class to illustrate Ros2cs in action </summary>
  public class HelloService
  {
    public static IService<hello_interfaces.srv.AddThreeInts_Request> my_service;

    public static void Main(string[] args)
    {
      Console.WriteLine("Hello Service start");
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("service");
      my_service = node.CreateService<hello_interfaces.srv.AddThreeInts_Request>(
        "add_three_ints", msg => { recv_callback (msg);});

      Ros2cs.Spin(node);
      Ros2cs.Shutdown();
    }

    public static void recv_callback ( hello_interfaces.srv.AddThreeInts_Request msg )
    {
      long sum = msg.A + msg.B + msg.C;
      Console.WriteLine ("Incoming Service Request A=" + msg.A + " B=" + msg.B + " C=" + msg.C);
      IntPtr psum = new IntPtr(sum);
      my_service.SendResp(psum);
    }
  }
}
