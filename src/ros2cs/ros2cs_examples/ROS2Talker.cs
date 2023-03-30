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

namespace Examples
{
    /// <summary> A simple talker class to illustrate Ros2cs in action </summary>
    public class ROS2Talker
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Talker starting");

            // everything is disposed when disposing the context
            using IContext context = new Context();
            context.TryCreateNode("talker", out INode node);
            IPublisher<std_msgs.msg.String> chatter_pub = node.CreatePublisher<std_msgs.msg.String>("chatter");
            std_msgs.msg.String msg = new std_msgs.msg.String();

            for (int i = 1; context.Ok(); i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                msg.Data = $"Hello World: {i}";
                Console.WriteLine(msg.Data);
                chatter_pub.Publish(msg);
            }
        }
    }
}
