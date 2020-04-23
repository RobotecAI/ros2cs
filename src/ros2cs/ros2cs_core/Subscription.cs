using System;

namespace ROS2
{
    public class Subscription<T>: ISubscription<T>
        where T : Message, new ()
    {
        private rcl_subscription_t handle;
        rcl_node_t nodeHandle;

        private IntPtr subscriptionOptions;
        public rcl_subscription_t Handle { get { return handle; } }
        internal Action<T> callback;

        private bool disposed;

        public Message CreateMessage()
        {
            return (Message)new T();
        }

        public void TriggerCallback(Message message)
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
                if(disposing)
                {
                    // Dispose managed resources.
                }

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
