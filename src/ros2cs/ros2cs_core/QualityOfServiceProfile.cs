using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROS2
{
    public enum QosProfiles
    {
       SENSOR_DATA,
       PARAMETERS,
       DEFAULT,
       SERVICES_DEFAULT,
       PARAMETER_EVENTS,
       SYSTEM_DEFAULT
    }

    public class QualityOfServiceProfile : IDisposable
    {
        private bool disposed;

        internal rmw_qos_profile_t handle;

        public QosProfiles Profile;

        public QualityOfServiceProfile(QosProfiles profile)
        {
            handle = new rmw_qos_profile_t();
            SetProfile(profile);
        }

        public void SetProfile(QosProfiles profile)
        {
            Profile = profile;
            switch(profile)
            {
                case QosProfiles.SENSOR_DATA:
                    SetProfileSensorData();
                    break;
                case QosProfiles.PARAMETERS:
                    SetProfileParameters();
                    break;
                case QosProfiles.DEFAULT:
                    SetProfileDefault();
                    break;
                case QosProfiles.SERVICES_DEFAULT:
                    SetProfileServicesDefault();
                    break;
                case QosProfiles.PARAMETER_EVENTS:
                    SetProfileParameterEvents();
                    break;
                case QosProfiles.SYSTEM_DEFAULT:
                    SetProfileSystemDefault();
                    break;
                default:
                    break;
            }
            SetProfileDefault();
        }

        public void SetTransientLocal(int depth = 10)
        {
            handle.depth = (ulong)depth;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_TRANSIENT_LOCAL;
        }

        private void SetProfileSensorData()
        {
            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 5;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_BEST_EFFORT;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        private void SetProfileParameters()
        {
            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 1000;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_RELIABLE;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        private void SetProfileDefault()
        {
            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 10;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_RELIABLE;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        private void SetProfileServicesDefault()
        {
            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 10;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_RELIABLE;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        private void SetProfileParameterEvents()
        {
            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 1000;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_RELIABLE;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        private void SetProfileSystemDefault()
        {

            handle.history = rmw_qos_history_policy_t.RMW_QOS_POLICY_HISTORY_KEEP_LAST;
            handle.depth = 1000;
            handle.reliability = rmw_qos_reliability_policy_t.RMW_QOS_POLICY_RELIABILITY_RELIABLE;
            handle.durability = rmw_qos_durability_policy_t.RMW_QOS_POLICY_DURABILITY_VOLATILE;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                //TODO(sam): dispose handle?
                disposed = true;
            }
        }
    }
}
