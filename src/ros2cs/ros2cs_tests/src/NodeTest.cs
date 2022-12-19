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
using example_interfaces.srv;

namespace ROS2.Test
{
    [TestFixture]
    public class NodeTest
    {
        INode node;
        string TEST_NODE = "my_node";

        [SetUp]
        public void SetUp()
        {
            Ros2cs.Init();
            node = Ros2cs.CreateNode(TEST_NODE);
        }

        [TearDown]
        public void TearDown()
        {
            node.Dispose();
            Ros2cs.Shutdown();
        }

        [Test]
        public void Accessors()
        {
            Assert.That(node.Name, Is.EqualTo("my_node"));
        }

        [Test]
        public void CreatePublisher()
        {
            Publisher<std_msgs.msg.Bool> publisher = node.CreatePublisher<std_msgs.msg.Bool>("test_topic");
            publisher.Dispose();

            using (publisher = node.CreatePublisher<std_msgs.msg.Bool>("test_topic"))
            {
            }
        }

        [Test]
        public void Publish()
        {
            using (Publisher<std_msgs.msg.Bool> publisher = node.CreatePublisher<std_msgs.msg.Bool>("test_topic"))
            {
                publisher.Publish(new std_msgs.msg.Bool());
            }
        }

        [Test]
        public void PublishChangingSize()
        {
          using (Publisher<test_msgs.msg.UnboundedSequences> publisher = node.CreatePublisher<test_msgs.msg.UnboundedSequences>("test_topic"))
          {
            string[] setStringSequence = new string[2]
            {
              "First",
              "Second string to send, has to be a bit longer for the test"
            };

            test_msgs.msg.UnboundedSequences msg3 = new test_msgs.msg.UnboundedSequences();
            msg3.String_values = setStringSequence;
            publisher.Publish(msg3);

            msg3.Int32_values = new int[2] { 1, 2 };
            msg3.String_values[0] = "A string that is longer than the previous one";
            msg3.String_values[1] = "shorter than previous one";

            // Publish reusing the message
            publisher.Publish(msg3);

            msg3.String_values = new string[5] { "1", "2", "3", "4", "5" };
            msg3.Int32_values = new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            publisher.Publish(msg3);

            msg3.String_values = new string[1] { "hello" };
            msg3.Int32_values = new int[1] { 1 };
            publisher.Publish(msg3);
          }
        }

        [Test]
        public void CreateSubscription()
        {
            Subscription<std_msgs.msg.Bool> subscription = node.CreateSubscription<std_msgs.msg.Bool>(
                "/subscription_topic", msg => Console.WriteLine("I heard: [" + msg.Data + "]"));
            subscription.Dispose();

            using (subscription = node.CreateSubscription<std_msgs.msg.Bool>(
                "test_topic", msg => Console.WriteLine("Got message")))
            {
            }
        }

        [Test]
        public void RemoveService()
        {
            var service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                "/test",
                request => { throw new InvalidOperationException("service should not be called"); }
            );
            
            Assert.That(node.RemoveService(service));
            Assert.That(service.IsDisposed);
        }

        [Test]
        public void RemoveClient()
        {
            var client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>("/test");
            
            Assert.That(node.RemoveClient(client));
            Assert.That(client.IsDisposed);
        }
    }
}
