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
  /// <summary> Public enum which can be used to acquire predefined qos configurations </summary>
  /// <remarks> This is mapped to rmw presets, for example SENSOR_DATA is rmw_qos_profile_sensor_data </remarks>
  public enum QosPresetProfile
  {
    SENSOR_DATA,
    PARAMETERS,
    DEFAULT,
    SERVICES_DEFAULT,
    PARAMETER_EVENTS,
    SYSTEM_DEFAULT
  }

  public enum HistoryPolicy
  {
    QOS_POLICY_HISTORY_SYSTEM_DEFAULT,
    QOS_POLICY_HISTORY_KEEP_LAST,
    QOS_POLICY_HISTORY_KEEP_ALL
  }

  public enum ReliabilityPolicy
  {
    QOS_POLICY_RELIABILITY_SYSTEM_DEFAULT,
    QOS_POLICY_RELIABILITY_RELIABLE,
    QOS_POLICY_RELIABILITY_BEST_EFFORT
  }

  public enum DurabilityPolicy
  {
    QOS_POLICY_DURABILITY_SYSTEM_DEFAULT,
    QOS_POLICY_DURABILITY_TRANSIENT_LOCAL,
    QOS_POLICY_DURABILITY_VOLATILE
  }

  public enum LivelinessPolicy
  {
    QOS_POLICY_LIVELINESS_SYSTEM_DEFAULT,
    QOS_POLICY_LIVELINESS_AUTOMATIC,
    QOS_POLICY_LIVELINESS_MANUAL_BY_TOPIC
  }

  /// <summary> Quality of Service settings for publishers and subscriptions </summary>
  public class QualityOfServiceProfile
  {
    internal IntPtr handle;

    /// <summary> Construct using a preset </summary>
    public QualityOfServiceProfile(QosPresetProfile preset_profile = QosPresetProfile.DEFAULT)
    {
      handle = NativeRmwInterface.rmw_native_interface_create_qos_profile((int)preset_profile);
    }

    public void SetHistory(HistoryPolicy policy, int depth)
    {
      NativeRmwInterface.rmw_native_interface_set_history(handle, (int)policy, depth);
    }

    public void SetReliability(ReliabilityPolicy policy)
    {
      NativeRmwInterface.rmw_native_interface_set_reliability(handle, (int)policy);
    }

    public void SetDurability(DurabilityPolicy policy)
    {
      NativeRmwInterface.rmw_native_interface_set_durability(handle, (int)policy);
    }
  }
}
