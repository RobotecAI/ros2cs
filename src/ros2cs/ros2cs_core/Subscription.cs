using System;

namespace ROS2
{
    public class Subscription<T>: ISubscription<T> where T : Message, new ()
    {
        public rcl_subscription_t Handle { get { return subscriptionHandle; } }
        private rcl_subscription_t subscriptionHandle;
        private rcl_node_t nodeHandle;

        private readonly Action<T> callback;
        private IntPtr subscriptionOptions;

        private bool disposed = false;
        private object mutex = new object();

        public void TakeMessage()
        {
          RCLReturnEnum ret;
          Message message;
          lock (mutex)
          {
            if (disposed || !Ros2cs.Ok())
            {
              return;
            }
            
            message = CreateMessage();
            ret = (RCLReturnEnum)NativeMethods.rcl_take(ref subscriptionHandle, message.Handle, IntPtr.Zero, IntPtr.Zero);
          }

          bool gotMessage = ret == RCLReturnEnum.RCL_RET_OK;

          if (gotMessage)
          {
              TriggerCallback(message);
          }
        }

        private Message CreateMessage()
        {
            return (Message)new T();
        }

        private void TriggerCallback(Message message)
        {
            message.ReadNativeMessage();
            callback((T)message);
        }

        public Subscription(string topic, Node node, Action<T> cb, QualityOfServiceProfile qos = null)
        {
            callback = cb;
            nodeHandle = node.nodeHandle;
            subscriptionHandle = NativeMethods.rcl_get_zero_initialized_subscription();

            QualityOfServiceProfile qualityOfServiceProfile = qos;
            if (qualityOfServiceProfile == null)
            {
              qualityOfServiceProfile = new QualityOfServiceProfile(QosProfiles.DEFAULT);
            }

            subscriptionOptions = NativeMethods.rclcs_subscription_create_options(qualityOfServiceProfile.handle);

            IMessageInternals msg = new T();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            msg.Dispose();

            Utils.CheckReturnEnum(NativeMethods.rcl_subscription_init(
                                    ref subscriptionHandle,
                                    ref node.nodeHandle,
                                    typeSupportHandle,
                                    topic,
                                    subscriptionOptions));
        }

        ~Subscription()
        {
            DestroySubscription();
        }

        public void Dispose()
        {
            DestroySubscription();
        }

        private void DestroySubscription()
        {
          lock (mutex)
          {
            if (!disposed)
            {
              Utils.CheckReturnEnum(NativeMethods.rcl_subscription_fini(ref subscriptionHandle, ref nodeHandle));
              NativeMethods.rclcs_node_dispose_options(subscriptionOptions);
              disposed = true;
              Ros2csLogger.GetInstance().LogInfo("Subscription destroyed");
            }
          }
        }
    }
}
