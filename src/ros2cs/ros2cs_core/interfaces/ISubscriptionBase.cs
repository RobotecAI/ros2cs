
namespace ROS2
{
    public interface ISubscriptionBase : System.IDisposable
    {
        rcl_subscription_t Handle { get; }
        Message CreateMessage();
        void TriggerCallback(Message message);
    }

}
