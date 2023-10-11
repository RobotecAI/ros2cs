// Copyright 2019-2023 Robotec.ai
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
using System.Collections;
using ROS2;
using ROS2.Executors;
using example_interfaces.srv;

namespace Examples
{
    /// <summary> A simple service class to illustrate Ros2cs in action </summary>
    public class ROS2Service
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Service start");

            // everything is disposed when disposing the context
            using Context context = new Context();
            using ManualExecutor executor = new ManualExecutor(context);
            context.TryCreateNode("service", out INode node);
            executor.Add(node);

            IService<AddTwoInts_Request, AddTwoInts_Response> my_service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                "add_two_ints",
                msg =>
                {
                    Console.WriteLine("Incoming Service Request A={0} B={1}", msg.A, msg.B);
                    AddTwoInts_Response response = new AddTwoInts_Response();
                    response.Sum = msg.A + msg.B;
                    return response;
                }
            );

            executor.SpinWhile(() => true);
        }
    }
}
