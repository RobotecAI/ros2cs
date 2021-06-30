using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ROS2
{
  internal static class WaitSet
  {
    private static rcl_allocator_t allocator;
    private static rcl_wait_set_t handle;

    static WaitSet()
    {
      allocator = NativeMethods.rcutils_get_default_allocator();
      handle = NativeMethods.rcl_get_zero_initialized_wait_set();
    }

    internal static void Wait(rcl_context_t context, List<ISubscriptionBase> subscriptions, double timeoutSec)
    {
      ulong numberOfSubscriptions;
      numberOfSubscriptions = (ulong)subscriptions.Count;

      if (numberOfSubscriptions == 0)
      {
          return;
      }

      ulong numberOfGuardConditions = 0;
      ulong numberOfTimers = 0;
      ulong numberOfClients = 0;
      ulong numberOfServices = 0;
      ulong numberOfEvents = 0;

      Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_init(
        ref handle,
        numberOfSubscriptions,
        numberOfGuardConditions,
        numberOfTimers,
        numberOfClients,
        numberOfServices,
        numberOfEvents,
        ref context,
        allocator));

      Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_clear(ref handle));

      ulong subscribtionsInWaitset = 0;
      foreach (ISubscriptionBase subscription in subscriptions)
      {
        // We only allocated for numberOfSubscriptions, so only add up to as many. Shouldn't happen.
        if (subscribtionsInWaitset >= numberOfSubscriptions)
        {
          // TODO - log warning
          break;
        }

        rcl_subscription_t subscription_handle = subscription.Handle;
        Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_add_subscription(ref handle, ref subscription_handle, UIntPtr.Zero));
        subscribtionsInWaitset++;
      }

      int rcl_wait_ret = NativeMethods.rcl_wait(ref handle, Utils.TimeoutSecToNsec(timeoutSec));
      if (rcl_wait_ret != (int)RCLReturnEnum.RCL_RET_OK && rcl_wait_ret != (int)RCLReturnEnum.RCL_RET_TIMEOUT)
      { // Timeout return is normal
        Utils.CheckReturnEnum(rcl_wait_ret);
      }
      Utils.CheckReturnEnum(NativeMethods.rcl_wait_set_fini(ref handle)); // wait_set is zero initialized again after this one
    }
  }
}
