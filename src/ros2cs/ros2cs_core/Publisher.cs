using System;
using System.Diagnostics;

namespace ROS2
{
  public class Publisher<T>: IPublisher<T> where T : Message, new ()
  {
    private Ros2csLogger logger = Ros2csLogger.GetInstance();
    rcl_publisher_t publisherHandle;
    IntPtr publisherOptions = IntPtr.Zero;
    rcl_node_t nodeHandle;
    private bool disposed = false;

    public Publisher(string topic, Node node, QualityOfServiceProfile qos = null)
    {
      nodeHandle = node.nodeHandle;

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
        qualityOfServiceProfile = new QualityOfServiceProfile(QosProfiles.DEFAULT);

      publisherOptions = NativeMethods.rclcs_publisher_create_options(qualityOfServiceProfile.handle);

      //TODO - do not create message to get type support handle
      IMessageInternals msg = new T();
      IntPtr typeSupportHandle = msg.TypeSupportHandle;
      msg.Dispose();

      publisherHandle = NativeMethods.rcl_get_zero_initialized_publisher();
      Utils.CheckReturnEnum(NativeMethods.rcl_publisher_init(
                              ref publisherHandle,
                              ref nodeHandle,
                              typeSupportHandle,
                              topic,
                              publisherOptions));
    }

    ~Publisher()
    {
      Dispose();
    }

    public void Dispose()
    {
      DestroyPublisher();
    }

    private void DestroyPublisher()
    {
      if (!disposed)
      {
        Utils.CheckReturnEnum(NativeMethods.rcl_publisher_fini(ref publisherHandle, ref nodeHandle));
        NativeMethods.rclcs_publisher_dispose_options(publisherOptions);
        logger.LogInfo("Publisher destroyed");
        disposed = true;
      }
    }

    public void Publish(T msg)
    {
      if (!Ros2cs.Ok() || disposed)
      {
        logger.LogWarning("Cannot publish as the class is already disposed or shutdown was called");
        return;
      }

      msg.WriteNativeMessage();
      Utils.CheckReturnEnum(NativeMethods.rcl_publish(ref publisherHandle, msg.Handle, IntPtr.Zero));
    }
  }
}
