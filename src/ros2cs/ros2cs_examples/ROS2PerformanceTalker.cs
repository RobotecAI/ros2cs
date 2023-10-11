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
using System.Threading;
using ROS2;
using sensor_msgs.msg;

namespace Examples
{
    /// <summary> A talker class meant to gauge performance of Ros2cs </summary>
    public class ROS2PerformanceTalker
    {
        private static Clock clock = new Clock();

        private static void AssignField(ref PointField pf, string n, uint off, byte dt, uint count)
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
            PointCloud2 message = new PointCloud2()
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
            message.Fields = new PointField[pointFieldCount];
            for (int i = 0; i < pointFieldCount; ++i)
            {
                message.Fields[i] = new PointField();
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
            using IContext context = new Context();
            context.TryCreateNode("perf_talker", out INode node);

            IPublisher<PointCloud2> pc_pub = node.CreatePublisher<PointCloud2>(
                "perf_chatter",
                new QualityOfServiceProfile(QosPresetProfile.SENSOR_DATA)
            );

            Console.WriteLine("Enter PC2 data size: ");
            sensor_msgs.msg.PointCloud2 msg = PrepMessage(Convert.ToInt32(Console.ReadLine()));

            while (context.Ok())
            {
                var nowTime = clock.Now;
                msg.UpdateHeaderTime(nowTime.sec, nowTime.nanosec);

                // Remove this benchmark if you want to measure maximum throughput for smallest messages
                using (var bench = new Benchmark("Publish"))
                {
                    // If we want to test changing sizes:
                    // msg = PrepMessage(rand.Next() / 1000);
                    pc_pub.Publish(msg);
                }
            }
        }
    }
}
