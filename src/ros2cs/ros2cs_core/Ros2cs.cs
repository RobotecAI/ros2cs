using System;
using ROS2;

namespace ROS2
{
    public static class Ros2cs
    {
        private static Context defaultGlobalContext;

        public static void Init()
        {
            defaultGlobalContext = new Context();
            Init(defaultGlobalContext);
        }

        public static void Shutdown()
        {
            Shutdown(defaultGlobalContext);
            defaultGlobalContext = null;
        }

        public static void Init(Context ctx)
        {
            ctx.Init();
        }

        public static void Shutdown(Context ctx)
        {
            ctx.Shutdown();
        }

        public static INode CreateNode(string nodeName, string ns = null)
        {
            return CreateNode(nodeName, defaultGlobalContext, ns);
        }

        public static INode CreateNode(string nodeName, Context ctx, string ns = null)
        {
            return new Node(nodeName, ctx, ns);
        }

        public static void Spin(INode node)
        {
            Spin(node, defaultGlobalContext);
        }

        public static void Spin(INode node, Context ctx)
        {
            while (Ok(ctx))
            {
                SpinOnce(node, 0.1);
            }
        }

        public static bool Ok()
        {
            return Ok(defaultGlobalContext);
        }

        public static bool Ok(Context ctx)
        {
            return (ctx != null && NativeMethods.rcl_context_is_valid(ref ctx.handle));
        }

        public static void SpinOnce(INode node, double timeoutSec)
        {
            SpinOnce(node, timeoutSec, defaultGlobalContext);
        }

        public static void SpinOnce(INode node, double timeoutSec, Context ctx)
        {
            if (timeoutSec > 0.0001d)
            {
                WaitSet waitSet = new WaitSet(ctx, node.Subscriptions);
                waitSet.Wait(timeoutSec);
            }

            foreach(ISubscriptionBase subscription in node.Subscriptions)
            {
                if (subscription == null)
                    continue; //Rare situation in which we are disposing, the snapshot was taken before clear() was called but after explicit Dispose()

                subscription.TakeMessage();
            }
        }

        public static String GetRMWImplementationID()
        {
            return MarshallingHelpers.PtrToString(NativeMethods.rmw_get_implementation_identifier());
        }
    }
}
