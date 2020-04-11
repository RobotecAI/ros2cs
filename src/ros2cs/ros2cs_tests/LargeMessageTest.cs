using NUnit.Framework;
using System;
using System.Linq;

namespace ROS2.Test
{
    [TestFixture]
    public class LargeMessageTest
    {
        Context publisherContext;
        Context subscriptionContext;
        Node subscriptionNode;
        Node publisherNode;
        Publisher<std_msgs.msg.Float64MultiArray> publisher;

        [SetUp]
        public void SetUp()
        {
            publisherContext = new Context();
            subscriptionContext = new Context();
            Ros2cs.Init(publisherContext);
            Ros2cs.Init(subscriptionContext);

            subscriptionNode = new Node("subscription_test_node", publisherContext);
            publisherNode = new Node("publisher_test_node", publisherContext);

            publisher = publisherNode.CreatePublisher<std_msgs.msg.Float64MultiArray>("subscription_test_topic");
        }

        [TearDown]
        public void TearDown()
        {
            publisher.Dispose();
            subscriptionNode.Dispose();
            Ros2cs.Shutdown(publisherContext);
        }
/*
        [Test]
        public void SubscriptionTriggerCallback()
        {
            bool callbackTriggered = false;
            subscriptionNode.CreateSubscription<std_msgs.msg.Float64MultiArray>("subscription_test_topic", (msg) => { callbackTriggered = true; });

            std_msgs.msg.Float64MultiArray largeMsg = new std_msgs.msg.Float64MultiArray();
            double[] data = new double[1024];
            largeMsg.data = data.OfType<double>().ToList();

            publisher.Publish(largeMsg);
            Ros2cs.SpinOnce(subscriptionNode, 0.1);

            Assert.That(callbackTriggered, Is.True);
        }
 */
    }
}
