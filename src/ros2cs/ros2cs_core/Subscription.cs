using System;
using ROS2.Internal;

namespace ROS2
{
    public class Subscription<T>: ISubscription<T> where T : Message, new ()
    {
        public rcl_subscription_t Handle { get { return subscriptionHandle; } }
        private rcl_subscription_t subscriptionHandle;

        public string Topic { get { return topic; } }
        private string topic;

        private rcl_node_t nodeHandle;

        private readonly Action<T> callback;
        private IntPtr subscriptionOptions;

        private bool disposed = false;
        private object mutex = new object();

        public void TakeMessage()
        {
          RCLReturnEnum ret;
          MessageInternals message;
          lock (mutex)
          {
            if (disposed || !Ros2cs.Ok())
            {
              return;
            }

            message = CreateMessage();
            ret = (RCLReturnEnum)NativeRcl.rcl_take(ref subscriptionHandle, message.Handle, IntPtr.Zero, IntPtr.Zero);
          }

          bool gotMessage = ret == RCLReturnEnum.RCL_RET_OK;

          if (gotMessage)
          {
              TriggerCallback(message);
          }
        }

        private MessageInternals CreateMessage()
        {
            return new T() as MessageInternals;
        }

        private void TriggerCallback(MessageInternals message)
        {
            message.ReadNativeMessage();
            callback((T)message);
        }

        public Subscription(string subTopic, Node node, Action<T> cb, QualityOfServiceProfile qos = null)
        {
            callback = cb;
            nodeHandle = node.nodeHandle;
            topic = subTopic;
            subscriptionHandle = NativeRcl.rcl_get_zero_initialized_subscription();

            QualityOfServiceProfile qualityOfServiceProfile = qos;
            if (qualityOfServiceProfile == null)
            {
              qualityOfServiceProfile = new QualityOfServiceProfile();
            }

            subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qualityOfServiceProfile.handle);

            T msg = new T();
            MessageInternals msgInternals = msg as MessageInternals;
            IntPtr typeSupportHandle = msgInternals.TypeSupportHandle;
            msg.Dispose();

            Utils.CheckReturnEnum(NativeRcl.rcl_subscription_init(
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
              Utils.CheckReturnEnum(NativeRcl.rcl_subscription_fini(ref subscriptionHandle, ref nodeHandle));
              NativeRclInterface.rclcs_node_dispose_options(subscriptionOptions);
              disposed = true;
              Ros2csLogger.GetInstance().LogInfo("Subscription destroyed");
            }
          }
        }
    }
}
