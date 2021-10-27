// Copyright 2019-2021 Robotec.ai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace ROS2
{
  /// <summary> Internal class wrapping rcl wait_set </summary>
  /// <description> A simple implementation with a static class, allowing for a single waitset </description>
  internal static class WaitSet
  {
    private static rcl_allocator_t allocator;
    private static rcl_wait_set_t handle;

    /// <summary> Static constructor which gets native defaults </summary>
    static WaitSet()
    {
      allocator = NativeRcl.rcutils_get_default_allocator();
      handle = NativeRcl.rcl_get_zero_initialized_wait_set();
    }

    /// <summary> Prepare wait set based on a list of subscriptions and call rcl_wait  </summary>
    /// <description> Handles initialization, clearing, populating and finalization in one go. WaitSet is
    /// used in Spin methods of Ros2cs </description>
    /// <param name="context"> Context for wait_set. Only subscriptions of nodes within context will be handled.
    /// Current library implementation only uses a single global context </param>
    /// <param name="subscriptions"> A list of subscriptions, can be from multiple nodes </param>
    /// <param name="timeoutSec"> Timeout in seconds to limit rcl_wait in case no executables are ready </param>
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

      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_init(
        ref handle,
        numberOfSubscriptions,
        numberOfGuardConditions,
        numberOfTimers,
        numberOfClients,
        numberOfServices,
        numberOfEvents,
        ref context,
        allocator));

      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_clear(ref handle));

      ulong subscribtionsInWaitset = 0;
      foreach (ISubscriptionBase subscription in subscriptions)
      {
        // We only allocated for numberOfSubscriptions, so only add up to as many. Shouldn't happen.
        if (subscribtionsInWaitset >= numberOfSubscriptions)
        {
          // TODO - log warning
          break;
        }

        if (subscription.IsDisposed)
        {
          continue;
        }

        rcl_subscription_t subscription_handle = subscription.Handle;
        Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_add_subscription(ref handle, ref subscription_handle, UIntPtr.Zero));
        subscribtionsInWaitset++;
      }

      ulong timeoutInNanoseconds = (ulong)(timeoutSec * 1000 * 1000 * 1000);
      int rcl_wait_ret = NativeRcl.rcl_wait(ref handle, timeoutInNanoseconds);
      if (rcl_wait_ret != (int)RCLReturnEnum.RCL_RET_OK && rcl_wait_ret != (int)RCLReturnEnum.RCL_RET_TIMEOUT)
      { // Timeout return is normal
        Utils.CheckReturnEnum(rcl_wait_ret);
      }
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_fini(ref handle)); // wait_set is zero initialized again after this one
    }
  }
}
