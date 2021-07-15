using System;

namespace ROS2
{
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

  public class QualityOfServiceProfile
  {
    internal IntPtr handle;

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
