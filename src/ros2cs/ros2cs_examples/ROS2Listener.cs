using System;
using ROS2;

namespace ConsoleApplication
{
    public class ROS2Listener
    {
        public static void Main(string[] args)
        {
            Context ctx = new Context();
            Ros2cs.Init(ctx);
            INode node = Ros2cs.CreateNode("listener", ctx);

            ISubscription<std_msgs.msg.String> chatter_sub = node.CreateSubscription<std_msgs.msg.String>(
              "chatter", msg => Console.WriteLine("I heard: [" + msg.Data + "]"));

            Ros2cs.Spin(node, ctx);
            Ros2cs.Shutdown(ctx);
        }
    }
}
