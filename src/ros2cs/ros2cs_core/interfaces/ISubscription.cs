using System;

namespace ROS2
{
    public interface ISubscriptionBase : IDisposable
    {
        void TakeMessage();
        rcl_subscription_t Handle {get;}
    }

    public interface ISubscription<T>: ISubscriptionBase where T: Message {}
}
