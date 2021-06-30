using System;
using System.Collections.Generic;

namespace ROS2
{
    public interface INode: IDisposable
    {
        string Name {get;}
        Publisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new();
        Subscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new();

        // returns whether something was actually removed
        bool RemovePublisher(IPublisherBase publisher);
        bool RemoveSubscription(ISubscriptionBase subscription);
    }
}
