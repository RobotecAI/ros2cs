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

namespace ROS2
{
  internal enum AddResult
  {
    SUCCESS,
    FULL,
    DISPOSED
  }

  internal class WaitSet
  {
    internal ulong SubscriptionCount {get { return Handle.size_of_subscriptions.ToUInt64(); }}

    internal ulong ClientCount {get { return Handle.size_of_clients.ToUInt64(); }}

    internal ulong ServiceCount {get { return Handle.size_of_services.ToUInt64(); }}

    private rcl_wait_set_t Handle;

    internal WaitSet(ref rcl_context_t context)
    {
      Handle = NativeRcl.rcl_get_zero_initialized_wait_set();
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_init(
        ref Handle,
        (UIntPtr)0,
        (UIntPtr)0,
        (UIntPtr)0,
        (UIntPtr)0,
        (UIntPtr)0,
        (UIntPtr)0,
        ref context,
        NativeRcl.rcutils_get_default_allocator()));
    }

    ~WaitSet()
    {
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_fini(ref Handle));
    }

    internal void Clear()
    {
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_clear(ref Handle));
    }

    internal void Resize(ulong subscriptionCount, ulong clientCount, ulong serviceCount)
    {
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_resize(
      ref Handle,
      (UIntPtr)subscriptionCount,
      (UIntPtr)0,
      (UIntPtr)0,
      (UIntPtr)clientCount,
      (UIntPtr)serviceCount,
      (UIntPtr)0));
    }

    internal AddResult TryAddSubscription(ISubscriptionBase subscription, out ulong index)
    {
      UIntPtr native_index = default(UIntPtr);
      int ret;
      lock (subscription.Mutex)
      {
        if (subscription.IsDisposed)
        {
          index = default(ulong);
          return AddResult.DISPOSED;
        }

        rcl_subscription_t subscription_handle = subscription.Handle;
        ret = NativeRcl.rcl_wait_set_add_subscription(
          ref Handle,
          ref subscription_handle,
          ref native_index
        );
      }

      if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_WAIT_SET_FULL)
      {
        index = default(ulong);
        return AddResult.FULL;
      }
      else
      {
        Utils.CheckReturnEnum(ret);
        index = native_index.ToUInt64();
        return AddResult.SUCCESS;
      }
    }

    internal AddResult TryAddClient(IClientBase client, out ulong index)
    {
      UIntPtr native_index = default(UIntPtr);
      int ret;
      lock (client.Mutex)
      {
        if (client.IsDisposed)
        {
          index = default(ulong);
          return AddResult.DISPOSED;
        }

        rcl_client_t client_handle = client.Handle;
        ret = NativeRcl.rcl_wait_set_add_client(
          ref Handle,
          ref client_handle,
          ref native_index
        );
      }
      
      if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_WAIT_SET_FULL)
      {
        index = default(ulong);
        return AddResult.FULL;
      }
      else
      {
        Utils.CheckReturnEnum(ret);
        index = native_index.ToUInt64();
        return AddResult.SUCCESS;
      }
    }

    internal AddResult TryAddService(IServiceBase service, out ulong index)
    {
      UIntPtr native_index = default(UIntPtr);
      int ret;

      lock (service.Mutex)
      {
        if (service.IsDisposed)
        {
          index = default(ulong);
          return AddResult.DISPOSED;
        }

        rcl_service_t service_handle = service.Handle;
        ret = NativeRcl.rcl_wait_set_add_service(
          ref Handle,
          ref service_handle,
          ref native_index
        );
      }


      if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_WAIT_SET_FULL)
      {
        index = default(ulong);
        return AddResult.FULL;
      }
      else
      {
        Utils.CheckReturnEnum(ret);
        index = native_index.ToUInt64();
        return AddResult.SUCCESS;
      }
    }

    internal bool Wait()
    {
      return Wait(TimeSpan.FromTicks(-1));
    }

    internal bool Wait(TimeSpan timeout)
    {
      int ret = NativeRcl.rcl_wait(ref Handle, timeout.Ticks * 100);
      if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_TIMEOUT)
      {
        return false;
      }
      else
      {
        Utils.CheckReturnEnum(ret);
        return true;
      }
    }
  }
}
