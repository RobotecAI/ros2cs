using System;

namespace ROS2
{
    public interface ISubscription<T>: ISubscriptionBase
    where T: Message {}
}
