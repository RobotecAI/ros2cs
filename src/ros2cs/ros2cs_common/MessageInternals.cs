using System;

namespace ROS2
{

namespace Internal
{ // a warning keyword where design did not deliver.

// TODO not sure if it is possible to make this internal
// Note that this has to be visible between message assemblies (e.g. calling for nested messages)
// as well as for all messages (custom, generated, in principle unknown to ros2cs_core).
// It also needs to be visible to ros2cs_core classes.
public interface MessageInternals
{
  IntPtr Handle { get; }
  IntPtr TypeSupportHandle { get; }
  void ReadNativeMessage();
  void WriteNativeMessage();
}

internal static class MessageTypeSupportHelper
{
  internal static IntPtr GetTypeSupportHandle<T>() where T : Message, new()
  {
    T msg = new T();
    IntPtr typeSupportHandle = (msg as MessageInternals).TypeSupportHandle;
    msg.Dispose();
    return typeSupportHandle;
  }
}

} // namespace Internal

} // namespace ROS2
