using System;

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

    //TODO - allow for detailed manipulation - expose rmw_qos_profile_t
    public class QualityOfServiceProfile
    {
        internal rmw_qos_profile_t handle;
        public QosProfiles Profile;


        //TODO - once the upgrade is there for FastRTPS, use liveliness as well
        public QualityOfServiceProfile(QosProfiles profile)
        {
            handle = new rmw_qos_profile_t();
            handle.liveliness = rmw_qos_liveliness_policy_t.RMW_QOS_POLICY_LIVELINESS_SYSTEM_DEFAULT;
            handle.liveliness_lease_duration.sec = 0;
            handle.liveliness_lease_duration.nsec = 0;

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

        public void SetLifespan(ulong sec, ulong nsec)
        {
            handle.lifespan.sec = sec;
            handle.lifespan.nsec = nsec;
        }

        public void SetDeadline(ulong sec, ulong nsec)
        {
            handle.deadline.sec = sec;
            handle.deadline.nsec = nsec;
        }

        public void SetLivelinessLeaseDuration(ulong sec, ulong nsec)
        {
            handle.liveliness_lease_duration.sec = sec;
            handle.liveliness_lease_duration.nsec = nsec;
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
    }
}
