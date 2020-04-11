using System;

namespace ROS2 {
  namespace Interfaces {
    //TODO - handle interfaces so there is not too much exposed on the library user side
    public interface IMessageInternals : System.IDisposable
    {
      IntPtr Handle { get; }
      IntPtr TypeSupportHandle { get; }
      void ReadNativeMessage();
      void WriteNativeMessage();
      void ReadNativeMessage(IntPtr handle);
      void WriteNativeMessage(IntPtr handle);
    }

    public interface Message : IMessageInternals
    {
    }

    public interface MessageWithHeader : Message
    { //An utility interface for messages with header
      void SetHeaderFrame(string frameID);
      string GetHeaderFrame();
      void UpdateHeaderTime(int sec, uint nanosec);
    }
  }
}
