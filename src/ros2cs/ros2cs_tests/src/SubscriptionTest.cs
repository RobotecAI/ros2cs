// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
// Copyright 2019-2021 Robotec.ai
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
    public class SubscriptionTest
    {
        private static readonly string TOPIC = "test_subscription";

        private Context Context;

        private INode Node;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
            Context.TryCreateNode("subscription_test_node", out Node);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        private std_msgs.msg.Int32 CreateMessage(int data)
        {
            var msg = new std_msgs.msg.Int32();
            msg.Data = data;
            return msg;
        }

        [Test]
        public void SubscriptionTriggerCallback()
        {
            bool callbackTriggered = false;
            using var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { callbackTriggered = true; }
            );
            Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC).Publish(CreateMessage(0));

            Assert.That(subscription.TryProcess());

            Assert.That(callbackTriggered, Is.True);
        }

        [Test]
        public void SubscriptionCallbackMessageData()
        {
            int messageData = 12345;
            using var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { messageData = msg.Data; }
            );
            Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC).Publish(CreateMessage(42));

            Assert.That(subscription.TryProcess());

            Assert.That(messageData, Is.EqualTo(42));
        }

        [Test]
        public void DisposedSubscriptionHandling()
        {
            var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { throw new InvalidOperationException($"received message {msg}"); }
            );
            
            Assert.That(subscription.IsDisposed, Is.False);

            subscription.Dispose();

            Assert.That(subscription.IsDisposed);
            Assert.That(Node.Subscriptions, Does.Not.Contain(subscription));
        }

        [Test]
        public void DoubleDisposeSubscription()
        {
            var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { throw new InvalidOperationException($"received message {msg}"); }
            );

            subscription.Dispose();
            subscription.Dispose();

            Assert.That(subscription.IsDisposed);
        }

        [Test]
        public void SubscriptionQosDefaultDepth()
        {
            int count = 0;
            using var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { count += 1; }
            );
            using var publisher = Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC);
            var msg = CreateMessage(42);

            for (int i = 0; i < 10; i++)
            {
                publisher.Publish(msg);
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.That(subscription.TryProcess());
            }
            Assert.That(subscription.TryProcess(), Is.False);

            Assert.That(count, Is.EqualTo(10));
        }

        [Test]
        public void SubscriptionQosSensorDataDepth()
        {
            int count = 0;
            using var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { count += 1; },
                new QualityOfServiceProfile(QosPresetProfile.SENSOR_DATA)
            );
            using var publisher = Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC);
            var msg = CreateMessage(42);

            for (int i = 0; i < 6; i++)
            {
                publisher.Publish(msg);
            }

            for (int i = 0; i < 5; i++)
            {
                Assert.That(subscription.TryProcess());
            }
            Assert.That(subscription.TryProcess(), Is.False);

            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public void SubscriptionTryProcessAsync()
        {
            bool callbackTriggered = false;
            using var subscription = Node.CreateSubscription<std_msgs.msg.Int32>(
                TOPIC,
                (msg) => { callbackTriggered = true; }
            );
            Node.CreatePublisher<std_msgs.msg.Int32>(TOPIC).Publish(CreateMessage(0));

            var task = subscription.TryProcessAsync();
            task.Wait();
            Assert.That(task.Result);

            Assert.That(callbackTriggered, Is.True);
        }
    }
}
