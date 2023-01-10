using System;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class PublisherTest
    {
        private static readonly string TOPIC = "test_publisher";

        private Context Context;

        private INode Node;

        private IPublisher<std_msgs.msg.Int32> Publisher;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
            Context.TryCreateNode("publisher_test_node", out Node);
            Publisher = Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        [Test]
        public void DisposedPublisherHandling()
        {
            Assert.That(Publisher.IsDisposed, Is.False);

            Publisher.Dispose();

            Assert.That(Publisher.IsDisposed);
            Assert.That(Node.Publishers, Does.Not.Contain(Publisher));
        }

        [Test]
        public void DoubleDisposePublisher()
        {
            Publisher.Dispose();
            Publisher.Dispose();

            Assert.That(Publisher.IsDisposed);
        }

        [Test]
        public void PublishDisposed()
        {
            var msg = new std_msgs.msg.Int32();
            msg.Data = 42;
            Publisher.Dispose();

            Assert.Throws<ObjectDisposedException>(() => Publisher.Publish(msg));
        }
    }
}
