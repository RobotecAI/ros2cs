
using System;

namespace ROS2
{
    public interface Message : IDisposable
    {
    }

    public interface MessageWithHeader : Message
    { //An utility interface for messages with header
      void SetHeaderFrame(string frameID);
      string GetHeaderFrame();
      void UpdateHeaderTime(int sec, uint nanosec);
    }
}  // namespace ROS2
