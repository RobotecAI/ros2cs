using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ROS2
{
    internal class WaitSet: IDisposable
    {
        private rcl_wait_set_t handle;
        private rcl_allocator_t allocator;
        private bool disposed = false;

        public WaitSet(Context ctx, List<ISubscriptionBase> subscriptions)
        {
            ulong numberOfSubscriptions;
            numberOfSubscriptions = (ulong)subscriptions.Count;

            ulong numberOfGuardConditions = 0;
            ulong numberOfTimers = 0;
            ulong numberOfClients = 0;
            ulong numberOfServices = 0;
            ulong numberOfEvents = 0;

            allocator = NativeMethods.rcutils_get_default_allocator();
            handle = NativeMethods.rcl_get_zero_initialized_wait_set();

            Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_init(
                ref handle,
                numberOfSubscriptions,
                numberOfGuardConditions,
                numberOfTimers,
                numberOfClients,
                numberOfServices,
                numberOfEvents,
                ref ctx.handle,
                allocator));

            Clear();

            ulong subscribtionsInWaitset = 0;
            foreach (ISubscriptionBase subscription in subscriptions)
            {
                // We only allocated for numberOfSubscriptions, so only add up to as many. Shouldn't happen now.
                if (subscribtionsInWaitset >= numberOfSubscriptions)
                    break;

                rcl_subscription_t subscription_handle = subscription.Handle;
                Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_add_subscription(ref handle, ref subscription_handle, UIntPtr.Zero));
                subscribtionsInWaitset++;
            }
        }

        public void Clear()
        {
            Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_clear(ref handle));
        }

        public void Wait(double timeoutSec)
        {
            NativeMethods.rcl_wait(ref handle, Utils.TimeoutSecToNsec(timeoutSec));
        }

        ~WaitSet()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(!disposed)
            {
                Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_fini(ref handle));
                disposed = true;
            }
        }
    }
}
