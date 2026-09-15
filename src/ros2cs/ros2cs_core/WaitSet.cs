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
using System.Runtime.InteropServices;

namespace ROS2
{
  internal enum AddResult
  {
    SUCCESS,
    FULL,
    DISPOSED
  }

  internal class WaitSet : IDisposable
  {
    internal ulong SubscriptionCount {get { return Handle.size_of_subscriptions.ToUInt64(); }}

    internal ulong ClientCount {get { return Handle.size_of_clients.ToUInt64(); }}

    internal ulong ServiceCount {get { return Handle.size_of_services.ToUInt64(); }}

    private rcl_wait_set_t Handle;
    private readonly List<GCHandle> entityHandles = new List<GCHandle>();
    private bool disposed;

    private IntPtr Retain<T>(T value) where T : struct
    {
      // rcl retains the address until the wait set is cleared. A ref to a local
      // copy is valid only during the add call, and is reused by subsequent adds.
      var pinned = GCHandle.Alloc(value, GCHandleType.Pinned);
      entityHandles.Add(pinned);
      return pinned.AddrOfPinnedObject();
    }

    private void ReleaseEntities()
    {
      foreach (var pinned in entityHandles) pinned.Free();
      entityHandles.Clear();
    }

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
      if (!disposed)
      {
        NativeRcl.rcl_wait_set_fini(ref Handle);
        ReleaseEntities();
      }
    }

    public void Dispose()
    {
      if (disposed) return;
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_fini(ref Handle));
      ReleaseEntities();
      disposed = true;
      GC.SuppressFinalize(this);
    }

    internal void Clear()
    {
      Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_clear(ref Handle));
      ReleaseEntities();
    }

    internal void Resize(ulong subscriptionCount, ulong clientCount, ulong serviceCount)
    {
      Clear();
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

        ret = NativeRcl.rcl_wait_set_add_subscription(
          ref Handle,
          Retain(subscription.Handle),
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

        ret = NativeRcl.rcl_wait_set_add_client(
          ref Handle,
          Retain(client.Handle),
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

        ret = NativeRcl.rcl_wait_set_add_service(
          ref Handle,
          Retain(service.Handle),
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
