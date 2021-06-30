using System;

namespace ROS2
{
    public interface IPublisherBase: IDisposable
    {
        string Topic {get;}  
    }

    public interface IPublisher<T>: IPublisherBase
        where T: Message
    {
        void Publish(T msg);
    }
}
