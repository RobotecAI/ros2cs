using System;
using ROS2;

namespace ConsoleApplication
{
    public class ROS2Listener
    {
        public static void Main(string[] args)
        {
            Ros2cs.Init();
            INode node = Ros2cs.CreateNode("listener");

            ISubscription<std_msgs.msg.String> chatter_sub = node.CreateSubscription<std_msgs.msg.String>(
              "chatter", msg => Console.WriteLine("I heard: [" + msg.Data + "]"));

            Ros2cs.Spin(node);
            Ros2cs.Shutdown();
        }
    }
}
