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
using System.Runtime.InteropServices;

namespace ROS2
{
  /// <summary>
  /// A set of structures to mimic rcl structs and be able to set fields without additional native calls
  /// <description>
  /// TODO (adamdbrw): this is a bit dangerous in that if structures change in a new ros2 version we can have
  /// a crash source. Consider removing in favor of handling all manipulation inside the native library,
  /// where structure changes will use defaults (new fields added) or produce compile-time errors (fields changed).
  /// </description>
  /// </summary>

  #pragma warning disable 0169

  public struct rcl_allocator_t
  {
    public IntPtr allocate;
    public IntPtr deallocate;
    public IntPtr reallocate;
    public IntPtr zero_allocate;
    public IntPtr state;
  }

  public struct rcl_arguments_t
  {
    private IntPtr impl;
  }

  public struct rcl_context_t
{
    private IntPtr global_arguments;
    private IntPtr impl;
    private IntPtr instance_id_storage;
  }

  public struct rcl_error_string_t
  {
    internal IntPtr str;
  }

  public struct rcl_init_options_t
  {
    private IntPtr impl;
  }

  public struct rcl_node_t
  {
    private IntPtr context;
    private IntPtr rcl_node_impl_t;
  }

  public struct rcl_publisher_t
  {
    private IntPtr impl;
  }

  public struct rcl_subscription_t
  {
    private IntPtr impl;
  }

  public struct rcl_client_t
  {
    private IntPtr impl;
  }

  public struct rcl_service_t
  {
    private IntPtr impl;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct rcl_rmw_request_id_t
  {
    /// The guid of the writer associated with this request
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] writer_guid;
    /// Sequence number of this service
    [MarshalAs(UnmanagedType.I8)]
    public long sequence_number;
  };

  public struct rcl_wait_set_t
  {
    private IntPtr subscriptions;
    internal UIntPtr size_of_subscriptions;
    private IntPtr guard_conditions;
    internal UIntPtr size_of_guard_conditions;
    private IntPtr timers;
    internal UIntPtr size_of_timers;
    private IntPtr clients;
    internal UIntPtr size_of_clients;
    private IntPtr services;
    internal UIntPtr size_of_services;
    private IntPtr events;
    internal UIntPtr size_of_events;
    private IntPtr impl;
  }

  public struct rcl_clock_t
  {
    private int type;
    private IntPtr jump_callbacks;
    private UIntPtr num_jump_callbacks;
    private IntPtr get_now;
    private IntPtr data;
    rcl_allocator_t allocator;
  }

#pragma warning restore 0169
}
