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

namespace ROS2.Test
{
    [TestFixture]
    public class LargeMessageTest
    {
        private Context Context;

        private INode SubscriptionNode;

        private INode PublisherNode;

        private IPublisher<std_msgs.msg.Float64MultiArray> Publisher;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();

            Context.TryCreateNode("subscription_test_node", out SubscriptionNode);
            Context.TryCreateNode("publisher_test_node", out PublisherNode);

            Publisher = PublisherNode.CreatePublisher<std_msgs.msg.Float64MultiArray>("subscription_test_topic");
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        [Test]
        public void SubscriptionTryProcess()
        {
            bool callbackTriggered = false;
            using var subscription = SubscriptionNode.CreateSubscription<std_msgs.msg.Float64MultiArray>(
                "subscription_test_topic", (msg) => { callbackTriggered = true; });

            std_msgs.msg.Float64MultiArray largeMsg = new std_msgs.msg.Float64MultiArray();
            largeMsg.Data = new double[1024];

            Publisher.Publish(largeMsg);
            
            Assert.That(subscription.TryProcess());

            Assert.That(callbackTriggered, Is.True);
        }
    }
}
