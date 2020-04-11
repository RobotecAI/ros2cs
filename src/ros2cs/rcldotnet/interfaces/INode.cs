using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ROS2.Interfaces;

namespace rclcs
{
    public interface INode: IDisposable
    {
        ConcurrentBag<ISubscriptionBase> Subscriptions { get; }
        Publisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new();
        Subscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new();
    }
}
