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
            IntPtr publisherOptions = NativeMethods.rclcs_publisher_create_default_options();
            int qosProfileRmw = qos == null ? (int)QosProfiles.DEFAULT : (int)qos.Profile;
            NativeMethods.rclcs_publisher_set_qos_profile(publisherOptions, qosProfileRmw);

            //MethodInfo m = typeof(T).GetTypeInfo().GetDeclaredMethod("_GET_TYPE_SUPPORT");
            //IntPtr typeSupportHandle = (IntPtr)m.Invoke(null, new object[] { });

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
            Utils.CheckReturnEnum(NativeMethods.rcl_publish(ref handle, msg.Handle));
        }
    }
}
