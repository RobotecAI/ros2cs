// Copyright 2019-2021 Robotec.ai
// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
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
  /// <summary> A simple structure to hold seconds and nanoseconds </summary>
  /// <description> This is meant to be an intermediate data object before time is packed into
  /// a rosgraph clock message or into a different format native to application layer </description>
  public struct RosTime
  {
    public int sec;
    public uint nanosec;

    public double Seconds
    {
      get { return sec + nanosec/1e9; }
    }
  }

  /// <summary> A clock class which queries an internal rcl clock and exposes RosTime </summary>
  public class Clock : IExtendedDisposable
  {
    internal IntPtr handle;
    private bool disposed;

    public bool IsDisposed { get { return disposed; } }

    /// <summary> Query current time </summary>
    /// <returns> Time in full seconds and nanoseconds </returns>
    public RosTime Now
    {
      get
      {
        RosTime time = new RosTime();
        long queryNowNanoseconds = 0;
        NativeRcl.rcl_clock_get_now(handle, ref queryNowNanoseconds);
        time.sec = (int)(queryNowNanoseconds / (long)1e9);
        time.nanosec = (uint)(queryNowNanoseconds - time.sec*((long)1e9));
        return time;
      }
    }

    public Clock()
    {
      rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
      handle = NativeRclInterface.rclcs_ros_clock_create(ref allocator);
    }

    ~Clock()
    {
      Dispose();
    }

    public void Dispose()
    {
      if (!disposed)
      {
        NativeRclInterface.rclcs_ros_clock_dispose(handle);
        disposed = true;
      }
    }
  }
}
