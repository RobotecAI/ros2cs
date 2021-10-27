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

using NUnit.Framework;
using System.Collections.Generic;

namespace ROS2.Test
{
    [TestFixture]
    public class SubscriptionTest
    {
        INode node;
        Publisher<std_msgs.msg.Int32> publisher;

        [SetUp]
        public void SetUp()
        {
            Ros2cs.Init();
            node = Ros2cs.CreateNode("subscription_test_node");
            publisher = node.CreatePublisher<std_msgs.msg.Int32>("subscription_test_topic");
        }

        [TearDown]
        public void TearDown()
        {
            publisher.Dispose();
            node.Dispose();
            Ros2cs.Shutdown();
        }

        [Test]
        public void SubscriptionTriggerCallback()
        {
            bool callbackTriggered = false;
            node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { callbackTriggered = true; });
            publisher.Publish(new std_msgs.msg.Int32());
            Ros2cs.SpinOnce(node, 0.1);

            Assert.That(callbackTriggered, Is.True);
        }

        [Test]
        public void SubscriptionCallbackMessageData()
        {
            int messageData = 12345;
            node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { messageData = msg.Data; });
            std_msgs.msg.Int32 published_msg = new std_msgs.msg.Int32();
            published_msg.Data = 42;
            publisher.Publish(published_msg);
            Ros2cs.SpinOnce(node, 0.1);

            Assert.That(messageData, Is.EqualTo(42));
        }

        [Test]
        public void DisposedSubscriptionHandling()
        {
            ISubscription<std_msgs.msg.Int32> subscriber =
              node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { });
            subscriber.Dispose();
            Assert.DoesNotThrow( () => { Ros2cs.SpinOnce(node, 0.1); });
        }

        [Test]
        public void MultipleDisposedSubscriptionsHandling()
        {
            int numberOfSubscribers = 10;
            List<Subscription<std_msgs.msg.Int32>> subscriptions = new List<Subscription<std_msgs.msg.Int32>>();
            for(int i = 0; i < numberOfSubscribers; i++)
            {
                subscriptions.Add(
                    node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { }));
            }
            Ros2cs.SpinOnce(node, 0.1);
            subscriptions.ForEach(delegate(Subscription<std_msgs.msg.Int32> subscription)
            {
                subscription.Dispose();
            });
            Assert.DoesNotThrow( () => { Ros2cs.SpinOnce(node, 0.1); });
        }

        [Test]
        public void ReinitializeDisposedSubscriber()
        {
            ISubscription<std_msgs.msg.Int32> subscriber =
              node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { });
            subscriber.Dispose();
            subscriber =
              node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic", (msg) => { });
            Assert.DoesNotThrow( () => { Ros2cs.SpinOnce(node, 0.1); });
        }

        [Test]
        public void SubscriptionQosDefaultDepth()
        {
            int count = 0;
            node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic",
                                                        (msg) => { count += 1; });

            std_msgs.msg.Int32 published_msg = new std_msgs.msg.Int32();
            published_msg.Data = 42;

            for (int i = 0; i < 10; i++)
            {
                publisher.Publish(published_msg);
            }

            for (int i = 0; i < 11; i++)
            {
                Ros2cs.SpinOnce(node, 0.1);
            }

            Assert.That(count, Is.EqualTo(10));
        }

        [Test]
        public void SubscriptionQosSensorDataDepth()
        {
            int count = 0;
            QualityOfServiceProfile qosProfile = 
                    new QualityOfServiceProfile(QosPresetProfile.SENSOR_DATA);

            node.CreateSubscription<std_msgs.msg.Int32>("subscription_test_topic",
                                                        (msg) => { count += 1; },
                                                        qosProfile);

            std_msgs.msg.Int32 published_msg = new std_msgs.msg.Int32();
            published_msg.Data = 42;

            for (int i = 0; i < 6; i++)
            {
                publisher.Publish(published_msg);
            }

            for (int i = 0; i < 11; i++)
            {
                Ros2cs.SpinOnce(node, 0.1);
            }

            Assert.That(count, Is.EqualTo(5));
        }
    }
}
