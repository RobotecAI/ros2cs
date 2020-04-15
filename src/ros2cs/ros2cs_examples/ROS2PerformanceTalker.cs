using System;
using System.Threading;
using ROS2;

namespace ConsoleApplication
{
    public class ROS2PerformanceTalker
    {
        private static Clock clock = new Clock();

        private static void AssignField(ref sensor_msgs.msg.PointField pf, string n, uint off, byte dt, uint count)
        {
            pf.Name = n;
            pf.Offset = off;
            pf.Datatype = dt;
            pf.Count = count;
        }

        private static sensor_msgs.msg.PointCloud2 PrepMessage(int messageSize)
        {
            uint count = (uint)messageSize; //point per message
            uint fieldsSize = 16;
            uint rowSize = count * fieldsSize;
            sensor_msgs.msg.PointCloud2 message = new sensor_msgs.msg.PointCloud2()
            {
                Height = 1,
                Width = count,
                Is_bigendian = false,
                Is_dense = true,
                Point_step = fieldsSize,
                Row_step = rowSize,
                Data = new byte[rowSize * 1]
            };
            uint pointFieldCount = 4;
            message.Fields = new sensor_msgs.msg.PointField[pointFieldCount];
            for (int i = 0; i < pointFieldCount; ++i)
            {
                message.Fields[i] = new sensor_msgs.msg.PointField();
            }

            AssignField(ref message.Fields[0], "x", 0, 7, 1);
            AssignField(ref message.Fields[1], "y", 4, 7, 1);
            AssignField(ref message.Fields[2], "z", 8, 7, 1);
            AssignField(ref message.Fields[3], "intensity", 12, 7, 1);
            float[] pointsArray = new float[count * message.Fields.Length];

            var floatIndex = 0;
            for (int i = 0; i < count; ++i)
            {
                float intensity = 100;
                pointsArray[floatIndex++] = 1;
                pointsArray[floatIndex++] = 2;
                pointsArray[floatIndex++] = 3;
                pointsArray[floatIndex++] = intensity;
            }
            System.Buffer.BlockCopy(pointsArray, 0, message.Data, 0, message.Data.Length);
            message.SetHeaderFrame("pc");
            return message;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Enter PC2 data size: ");
            int messageSize = Convert.ToInt32(Console.ReadLine());
            Context ctx = new Context();
            Ros2cs.Init(ctx);
            INode node = Ros2cs.CreateNode("perf_talker", ctx);
            QualityOfServiceProfile qos = new QualityOfServiceProfile(QosProfiles.SENSOR_DATA);
            IPublisher<sensor_msgs.msg.PointCloud2> pc_pub = node.CreatePublisher<sensor_msgs.msg.PointCloud2>("perf_chatter", qos);
            sensor_msgs.msg.PointCloud2 msg = PrepMessage(messageSize);

/*
            IPublisher<std_msgs.msg.String> chatter_pub = node.CreatePublisher<std_msgs.msg.String>("chatter");
            std_msgs.msg.String msg = new std_msgs.msg.String();

            int i = 1;
*/
            while (Ros2cs.Ok(ctx))
            {
                // adamdbrw - if at least small sleep is not made before
                // the first published message, it doesn't reach subscribers
                // Needs investigation for how to put a valid check here
                // TODO (adam) replace with a proper check
                // Thread.Sleep(1); //10ms

                var nowTime = clock.Now;
                msg.UpdateHeaderTime(nowTime.sec, nowTime.nanosec);
                //msg.Data = "Hello World: " + i;
                //i++;
                // Console.WriteLine("Publishing ");
                pc_pub.Publish(msg);
            }
            Ros2cs.Shutdown(ctx);
        }
    }
}
