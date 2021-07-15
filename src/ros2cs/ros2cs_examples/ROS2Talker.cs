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
            Console.WriteLine("Talker starting");
            Ros2cs.Init();
            INode node = Ros2cs.CreateNode("talker");
            Publisher<std_msgs.msg.String> chatter_pub = node.CreatePublisher<std_msgs.msg.String>("chatter");
            std_msgs.msg.String msg = new std_msgs.msg.String();

            int i = 1;

            while (Ros2cs.Ok())
            {
                Thread.Sleep(1000); //1s
                msg.Data = "Hello World: " + i;
                i++;
                Console.WriteLine(msg.Data);
                chatter_pub.Publish(msg);
            }
            Ros2cs.Shutdown();
        }
    }
}
