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
        private bool disposed;

        public WaitSet(Context ctx, ConcurrentBag<ISubscriptionBase> subscriptions = null)
        {
            ulong numberOfSubscriptions;

            if (subscriptions == null)
            {
                numberOfSubscriptions = 0;
                subscriptions = new ConcurrentBag<ISubscriptionBase>();
            }
            else
            {
                numberOfSubscriptions = (ulong)subscriptions.Count;
            }

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

            foreach (ISubscriptionBase subscription in subscriptions)
            {
                rcl_subscription_t subscription_handle = subscription.Handle;
                Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_add_subscription(ref handle, ref subscription_handle, UIntPtr.Zero));
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
