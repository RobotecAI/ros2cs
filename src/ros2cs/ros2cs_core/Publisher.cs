using System;
using System.Diagnostics;

namespace ROS2
{
    public class Publisher<T>: IPublisher<T> where T : Message, new ()
    {
        rcl_publisher_t handle;
        rcl_node_t nodeHandle;

        private IntPtr publisherOptions = new IntPtr();
        private bool disposed;

        public Publisher(string topic, Node node, QualityOfServiceProfile qos = null)
        {
            nodeHandle = node.handle;
            handle = NativeMethods.rcl_get_zero_initialized_publisher();
            int qosProfileRmw = qos == null ? (int)QosProfiles.DEFAULT : (int)qos.Profile;

            QualityOfServiceProfile qualityOfServiceProfile = qos;
            if (qualityOfServiceProfile == null)
                qualityOfServiceProfile = new QualityOfServiceProfile(QosProfiles.DEFAULT);

            IntPtr publisherOptions = NativeMethods.rclcs_publisher_create_options(qualityOfServiceProfile.handle);

            //TODO - do not create message to get type support handle
            IMessageInternals msg = new T();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            msg.Dispose();
            Utils.CheckReturnEnum(NativeMethods.rcl_publisher_init(
                                    ref handle,
                                    ref nodeHandle,
                                    typeSupportHandle,
                                    topic,
                                    publisherOptions));
        }

        ~Publisher()
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

                DestroyPublisher();
                disposed = true;
            }
        }

        private void DestroyPublisher()
        {
            Utils.CheckReturnEnum(NativeMethods.rcl_publisher_fini(ref handle, ref nodeHandle));
            NativeMethods.rclcs_publisher_dispose_options(publisherOptions);
        }

        public void Publish(T msg)
        {
            msg.WriteNativeMessage();
            Utils.CheckReturnEnum(NativeMethods.rcl_publish(ref handle, msg.Handle, IntPtr.Zero));
        }
    }
}
