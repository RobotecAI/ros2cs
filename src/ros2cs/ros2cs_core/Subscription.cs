using System;

namespace ROS2
{
    public class Subscription<T>: ISubscription<T> where T : Message, new ()
    {
        public rcl_subscription_t Handle { get { return handle; } }
        internal Action<T> callback;

        private IntPtr subscriptionOptions;
        private rcl_subscription_t handle;
        private rcl_node_t nodeHandle;
        private bool disposed;

        public void TakeMessage()
        {
            Message message = CreateMessage();
            IntPtr message_handle = message.Handle;
            RCLReturnEnum ret = (RCLReturnEnum)NativeMethods.rcl_take(ref handle, message_handle, IntPtr.Zero, IntPtr.Zero);
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

        public Subscription(string topic, Node node, Action<T> callback, QualityOfServiceProfile qos = null)
        {
            this.callback = callback;
            nodeHandle = node.handle;
            handle = NativeMethods.rcl_get_zero_initialized_subscription();

            QualityOfServiceProfile qualityOfServiceProfile = qos;
            if (qualityOfServiceProfile == null)
                qualityOfServiceProfile = new QualityOfServiceProfile(QosProfiles.DEFAULT);

            subscriptionOptions = NativeMethods.rclcs_subscription_create_options(qualityOfServiceProfile.handle);

            IMessageInternals msg = new T();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            msg.Dispose();

            Utils.CheckReturnEnum(NativeMethods.rcl_subscription_init(
                                    ref handle,
                                    ref nodeHandle,
                                    typeSupportHandle,
                                    topic,
                                    subscriptionOptions));
        }

        ~Subscription()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                DestroySubscription();
                disposed = true;
            }
        }

        private void DestroySubscription()
        {
            Utils.CheckReturnEnum(NativeMethods.rcl_subscription_fini(ref handle, ref nodeHandle));
            NativeMethods.rclcs_node_dispose_options(subscriptionOptions);
        }
    }
}
