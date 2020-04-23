using System;
using System.Threading;
using ROS2;
using std_msgs;

namespace ConsoleApplication
{
    public class ROS2Talker
    {

        public static void Main(string[] args)
        {
            Context ctx = new Context();
            Ros2cs.Init(ctx);
            INode node = Ros2cs.CreateNode("talker", ctx);
            Publisher<std_msgs.msg.String> chatter_pub = node.CreatePublisher<std_msgs.msg.String>("chatter");
            std_msgs.msg.String msg = new std_msgs.msg.String();
            Console.WriteLine("Using RMW implementation: " + Ros2cs.GetRMWImplementationID());

            int i = 1;

            while (Ros2cs.Ok(ctx))
            {
                Thread.Sleep(1000); //1s
                msg.Data = "Hello World: " + i;
                i++;
                Console.WriteLine(msg.Data);
                chatter_pub.Publish(msg);
            }
            Ros2cs.Shutdown(ctx);
        }
    }
}
