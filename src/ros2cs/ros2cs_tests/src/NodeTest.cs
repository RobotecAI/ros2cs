// Copyright 2019-2023 Robotec.ai
// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
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
using example_interfaces.srv;
using NUnit.Framework;
using ROS2.Executors;

namespace ROS2.Test
{
    [TestFixture]
    public class NodeTest
    {
        private static readonly string TEST_NODE = "my_node";

        private Context Context;

        private INode Node;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
            Context.TryCreateNode(TEST_NODE, out Node);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        [Test]
        public void NameProperty()
        {
            Assert.That(Node.Name, Is.EqualTo(TEST_NODE));
        }

        [Test]
        public void ContextProperty()
        {
            Assert.That(Object.ReferenceEquals(Node.Context, Context));
        }

        [Test]
        public void DefaultExecutor()
        {
            Assert.That(Node.Executor, Is.Null);
        }

        [Test]
        public void IsDisposed()
        {
            Assert.That(Node.IsDisposed, Is.False);

            Node.Dispose();

            Assert.That(Node.IsDisposed);
        }

        [Test]
        public void DisposedPublisherCreation()
        {
            Node.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                Node.CreatePublisher<std_msgs.msg.Bool>("test_publisher");
            });
            Assert.That(Node.Publishers.Count, Is.Zero);
        }

        [Test]
        public void DisposedSubscriptionCreation()
        {
            Node.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                Node.CreateSubscription<std_msgs.msg.Bool>(
                    "test_subscription",
                    msg => { throw new InvalidOperationException($"subscription called with {msg}"); }
                );
            });
            Assert.That(Node.Subscriptions.Count, Is.Zero);
        }

        [Test]
        public void DisposedServiceCreation()
        {
            Node.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                    "service_test",
                    request => { throw new InvalidOperationException($"received request {request}"); }
                );
            });
            Assert.That(Node.Services.Count, Is.Zero);
        }

        [Test]
        public void DisposedClientCreation()
        {
            Node.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>("client_test");
            });
            Assert.That(Node.Clients.Count, Is.Zero);
        }

        [Test]
        public void DoubleDisposal()
        {
            Node.Dispose();
            Node.Dispose();

            Assert.That(Node.IsDisposed);
        }

        [Test]
        public void DisposeAllOnDispose()
        {
            var publisher = Node.CreatePublisher<std_msgs.msg.Bool>("publisher_topic");
            var subscription = Node.CreateSubscription<std_msgs.msg.Bool>(
                "publisher_topic",
                msg => { throw new InvalidOperationException($"received message {msg}"); }
            );
            var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                "service_topic",
                request => { throw new InvalidOperationException($"received request {request}"); }
            );
            var client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
                "service_topic"
            );

            Node.Dispose();

            Assert.That(publisher.IsDisposed);
            Assert.That(subscription.IsDisposed);
            Assert.That(service.IsDisposed);
            Assert.That(client.IsDisposed);
            Assert.That(Node.Publishers, Is.Empty);
            Assert.That(Node.Subscriptions, Is.Empty);
            Assert.That(Node.Services, Is.Empty);
            Assert.That(Node.Clients, Is.Empty);
        }

        [Test]
        public void RemoveExecutorOnDispose()
        {
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            executor.Rescan();

            this.Node.Dispose();

            Assert.That(executor, Is.Empty);
            Assert.That(this.Node.Executor, Is.Null);
        }

        [Test]
        public void CreatePublisher()
        {
            string topic = "publisher_topic";
            var publishers = Node.Publishers;
            using IPublisher<std_msgs.msg.Bool> publisher = Node.CreatePublisher<std_msgs.msg.Bool>(topic);
            
            Assert.That(publishers, Contains.Item(publisher));
            Assert.That(publisher.Topic, Is.EqualTo(topic));
        }

        [Test]
        public void CreatePublisherWithExecutor()
        {
            string topic = "publisher_topic";
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            executor.Rescan();

            using IPublisher<std_msgs.msg.Bool> publisher = Node.CreatePublisher<std_msgs.msg.Bool>(topic);

            // publisher not in executor
            Assert.That(executor.RescanScheduled, Is.False);
        }

        [Test]
        public void DisposePublisher()
        {
            IPublisher<std_msgs.msg.Bool> publisher = Node.CreatePublisher<std_msgs.msg.Bool>("test_topic");
        
            Assert.That(Node.Publishers, Contains.Item(publisher));

            publisher.Dispose();

            Assert.That(Node.Publishers, Does.Not.Contain(publisher));
        }

        [Test]
        public void DisposePublisherWithExecutor()
        {
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            IPublisher<std_msgs.msg.Bool> publisher = Node.CreatePublisher<std_msgs.msg.Bool>("test_topic");
            executor.Rescan();

            publisher.Dispose();

            // publisher not in executor
            Assert.That(executor.RescanScheduled, Is.False);
        }

        [Test]
        public void CreateSubscription()
        {
            string topic = "subscription_topic";
            var subscriptions = Node.Subscriptions;
            using ISubscription<std_msgs.msg.Bool> subscription = Node.CreateSubscription<std_msgs.msg.Bool>(
                topic,
                msg => { throw new InvalidOperationException($"received message {msg}"); }
            );

            Assert.That(subscriptions, Contains.Item(subscription));
            Assert.That(subscription.Topic, Is.EqualTo(topic));
        }

        [Test]
        public void CreateSubscriptionWithExecutor()
        {
            string topic = "subscription_topic";
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            executor.Rescan();

            using ISubscription<std_msgs.msg.Bool> subscription = Node.CreateSubscription<std_msgs.msg.Bool>(
                topic,
                msg => { throw new InvalidOperationException($"received message {msg}"); }
            );

            Assert.That(executor.RescanScheduled, Is.True);
        }

        [Test]
        public void DisposeSubscription()
        {
            ISubscription<std_msgs.msg.Bool> subscription = Node.CreateSubscription<std_msgs.msg.Bool>(
                "test_topic",
                msg => { throw new InvalidOperationException($"received message {msg}"); }
            );

            Assert.That(Node.Subscriptions, Contains.Item(subscription));

            subscription.Dispose();

            Assert.That(Node.Subscriptions, Does.Not.Contain(subscription));
        }

        [Test]
        public void DisposeSubscriptionWithExecutor()
        {
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            ISubscription<std_msgs.msg.Bool> subscription = Node.CreateSubscription<std_msgs.msg.Bool>(
                "test_topic",
                msg => { throw new InvalidOperationException($"received message {msg}"); }
            );
            executor.Rescan();

            subscription.Dispose();

            Assert.That(executor.RescanScheduled, Is.True);
        }

        [Test]
        public void CreateService()
        {
            string topic = "service_topic";
            var services = Node.Services;
            using IService<AddTwoInts_Request, AddTwoInts_Response> service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                topic,
                request => { throw new InvalidOperationException($"received request {request}"); }
            );

            Assert.That(services, Contains.Item(service));
            Assert.That(service.Topic, Is.EqualTo(topic));
        }

        [Test]
        public void CreateServiceWithExecutor()
        {
            string topic = "service_topic";
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            executor.Rescan();

            using IService<AddTwoInts_Request, AddTwoInts_Response> service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                topic,
                request => { throw new InvalidOperationException($"received request {request}"); }
            );

            Assert.That(executor.RescanScheduled, Is.True);
        }

        [Test]
        public void DisposeService()
        {
            IService<AddTwoInts_Request, AddTwoInts_Response> service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                "test_topic",
                request => { throw new InvalidOperationException($"received request {request}"); }
            );
            
            Assert.That(Node.Services, Contains.Item(service));

            service.Dispose();

            Assert.That(Node.Services, Does.Not.Contain(service));
        }

        [Test]
        public void DisposeServiceWithExecutor()
        {
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            IService<AddTwoInts_Request, AddTwoInts_Response> service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                "test_topic",
                request => { throw new InvalidOperationException($"received request {request}"); }
            );
            executor.Rescan();

            service.Dispose();

            Assert.That(executor.RescanScheduled, Is.True);
        }

        [Test]
        public void CreateClient()
        {
            string topic = "client_topic";
            var clients = Node.Clients;
            using IClient<AddTwoInts_Request, AddTwoInts_Response> client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
                topic
            );

            Assert.That(clients, Contains.Item(client));
            Assert.That(client.Topic, Is.EqualTo(topic));
        }

        [Test]
        public void CreateClientWithExecutor()
        {
            string topic = "client_topic";
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            executor.Rescan();

            using IClient<AddTwoInts_Request, AddTwoInts_Response> client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
                topic
            );

            Assert.That(executor.RescanScheduled, Is.True);
        }

        [Test]
        public void DisposeClient()
        {
            IClient<AddTwoInts_Request, AddTwoInts_Response> client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
                "test_topic"
            );
            
            Assert.That(Node.Clients, Contains.Item(client));

            client.Dispose();

            Assert.That(Node.Clients, Does.Not.Contain(client));
        }

        [Test]
        public void DisposeClientWithExecutor()
        {
            using var executor = new ManualExecutor(this.Context);
            executor.Add(this.Node);
            IClient<AddTwoInts_Request, AddTwoInts_Response> client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(
                "test_topic"
            );
            executor.Rescan();

            client.Dispose();

            Assert.That(executor.RescanScheduled, Is.True);
        }
    }
}
