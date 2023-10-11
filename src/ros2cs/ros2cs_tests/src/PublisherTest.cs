// Copyright 2023 ADVITEC Informatik GmbH - www.advitec.de
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
