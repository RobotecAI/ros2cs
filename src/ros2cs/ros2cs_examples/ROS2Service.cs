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

namespace Examples
{
  /// <summary> A simple service class to illustrate Ros2cs in action </summary>
  public class ROS2Service
  {
    public static IService<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response> my_service;

    public static void Main(string[] args)
    {
      Console.WriteLine("Service start");
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("service");
      my_service = node.CreateService<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response>(
        "add_two_ints", recv_callback);

      Ros2cs.Spin(node);
      Ros2cs.Shutdown();
    }

    public static example_interfaces.srv.AddTwoInts_Response recv_callback( example_interfaces.srv.AddTwoInts_Request msg )
    {
      Console.WriteLine ("Incoming Service Request A=" + msg.A + " B=" + msg.B);
      example_interfaces.srv.AddTwoInts_Response response = new example_interfaces.srv.AddTwoInts_Response();
      response.Sum = msg.A + msg.B;
      return response;
    }
  }
}
