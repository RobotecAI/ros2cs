using System;
using ROS2.Interfaces;
using ROS2.Common;

namespace rclcs
{
    public static class Rclcs
    {
        public static void Init(Context context)
        {
            context.Init();
        }

        public static void Shutdown(Context context)
        {
            context.Shutdown();
        }

        public static Node CreateNode(string nodeName, Context context, string nodeNamespace = null)
        {
            return new Node(nodeName, context, nodeNamespace: nodeNamespace);
        }

        public static void Spin(INode node, Context context)
        {
            while (Ok(context))
            {
                SpinOnce(node, context, 0.1);
            }
        }

        public static bool Ok(Context context)
        {
            return NativeMethods.rcl_context_is_valid(ref context.handle);
        }

        public static void SpinOnce(INode node, Context context, double timeoutSec)
        {
            if (timeoutSec > 0.0001d)
            {
                WaitSet waitSet = new WaitSet(context, node.Subscriptions);
                waitSet.Wait(timeoutSec);
            }

            foreach(ISubscriptionBase subscription in node.Subscriptions)
            {
                if (subscription == null)
                    continue; //Rare situation in which we are disposing, the snapshot was taken before clear() was called but after explicit Dispose()

                Message message = subscription.CreateMessage();
                bool gotMessage = Take(subscription, message);

                if (gotMessage)
                {
                    subscription.TriggerCallback(message);
                }
            }
        }

        public static bool Take(ISubscriptionBase subscription, Message message)
        {
            rcl_subscription_t subscription_handle = subscription.Handle;
            IntPtr message_handle = message.Handle;
            RCLReturnEnum ret = (RCLReturnEnum)NativeMethods.rcl_take(ref subscription_handle, message_handle, IntPtr.Zero);
            return ret == RCLReturnEnum.RCL_RET_OK;
        }
    }
}
