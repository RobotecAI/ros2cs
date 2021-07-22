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
  /// <summary> A talker class meant to gauge performance of Ros2cs </summary>
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
      Ros2cs.Init();
      INode node = Ros2cs.CreateNode("perf_talker");
      QualityOfServiceProfile qos = new QualityOfServiceProfile(QosPresetProfile.SENSOR_DATA);
      IPublisher<sensor_msgs.msg.PointCloud2> pc_pub = node.CreatePublisher<sensor_msgs.msg.PointCloud2>("perf_chatter", qos);

      Console.WriteLine("Enter PC2 data size: ");
      int messageSize = Convert.ToInt32(Console.ReadLine());
      sensor_msgs.msg.PointCloud2 msg = PrepMessage(messageSize);
      // System.Random rand = new System.Random();

      while (Ros2cs.Ok())
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
      Ros2cs.Shutdown();
    }
  }
}
