using ROS2.Interfaces;

namespace rclcs
{
    public interface ISubscriptionBase : System.IDisposable
    {
        rcl_subscription_t Handle { get; }
        Message CreateMessage();
        void TriggerCallback(Message message);
    }

}
