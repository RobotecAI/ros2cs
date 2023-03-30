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
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using ROS2;
using ROS2.Executors;
using example_interfaces.srv;

namespace Examples
{
    /// <summary> A simple service client class to illustrate Ros2cs in action </summary>
    public class ROS2Client
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Client start");

            // everything is disposed when disposing the context
            using Context context = new Context();
            using ManualExecutor executor = new ManualExecutor(context);
            context.TryCreateNode("client", out INode node);
            executor.Add(node);

            IClient<AddTwoInts_Request, AddTwoInts_Response> my_client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>("add_two_ints");
            AddTwoInts_Request msg = new AddTwoInts_Request();
            msg.A = 7;
            msg.B = 2;

            while (!my_client.IsServiceAvailable())
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.25));
            }

            Task<AddTwoInts_Response> rsp = my_client.CallAsync(msg);
            for (IEnumerator spin = executor.Spin(TimeSpan.FromSeconds(0.1)); spin.MoveNext();)
            {
                if (rsp.IsCompleted)
                {
                    break;
                }
            }
            
            Console.WriteLine("Sum = {0}", rsp.Result.Sum);
        }
    }
}
