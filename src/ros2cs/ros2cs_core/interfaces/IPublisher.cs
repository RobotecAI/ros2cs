using System;

namespace ROS2
{
    public interface IPublisher<T>: IPublisherBase
        where T: Message
    {
        void Publish(T msg);
    }
}
