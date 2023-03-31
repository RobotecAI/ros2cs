using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using example_interfaces.srv;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class WaitSetTest
    {
        private static readonly string SUBSCRIPTION_TOPIC = "test_subscription";

        private static readonly string SERVICE_TOPIC = "test_service";

        private Context Context;

        private WaitSet WaitSet;

        [SetUp]
        public void SetUp()
        {
            this.Context = new Context();
            this.WaitSet = new WaitSet(this.Context);
        }

        [TearDown]
        public void TearDown()
        {
            this.Context.Dispose();
        }

        [Test]
        public void DisposedWaitSetHandling()
        {
            Assert.That(this.WaitSet.IsDisposed, Is.False);

            this.Context.Dispose();

            Assert.That(this.WaitSet.IsDisposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => this.WaitSet.TryWait(TimeSpan.Zero, out _));
        }

        [Test]
        public void DoubleDisposeWaitSet()
        {
            this.WaitSet.Dispose();
            this.WaitSet.Dispose();

            Assert.That(this.WaitSet.IsDisposed, Is.True);
        }

        [Test]
        public void TestSubscriptionCollection()
        {
            Assert.That(this.WaitSet.Count, Is.Zero);

            this.Context.TryCreateNode("TestNode", out var node);
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { throw new InvalidOperationException($"callback was triggered with {msg}"); }
            );

            this.WaitSet.Subscriptions.Add(subscription);

            Assert.That(this.WaitSet.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Subscriptions, Does.Contain(subscription));
    
            Assert.That(this.WaitSet.Subscriptions.Remove(subscription), Is.True);

            Assert.That(this.WaitSet.Subscriptions, Does.Not.Contain(subscription));
        }

        [Test]
        public void TestClientCollection()
        {
            Assert.That(this.WaitSet.Count, Is.Zero);

            this.Context.TryCreateNode("TestNode", out var node);
            using var client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_TOPIC);

            this.WaitSet.Clients.Add(client);

            Assert.That(this.WaitSet.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Clients.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Clients, Does.Contain(client));
    
            Assert.That(this.WaitSet.Clients.Remove(client), Is.True);

            Assert.That(this.WaitSet.Clients, Does.Not.Contain(client));
        }

        [Test]
        public void TestServiceCollection()
        {
            Assert.That(this.WaitSet.Count, Is.Zero);

            this.Context.TryCreateNode("TestNode", out var node);
            using var service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_TOPIC,
                request => { throw new InvalidOperationException($"received request ${request}"); }
            );

            this.WaitSet.Services.Add(service);

            Assert.That(this.WaitSet.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Services.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.Services, Does.Contain(service));
    
            Assert.That(this.WaitSet.Services.Remove(service), Is.True);

            Assert.That(this.WaitSet.Services, Does.Not.Contain(service));
        }

        [Test]
        public void TestGuardConditionCollection()
        {
            Assert.That(this.WaitSet.Count, Is.Zero);

            using var guard_condition = new GuardCondition(
                this.Context,
                () => throw new InvalidOperationException("guard condition was triggered!")
            );
            this.WaitSet.GuardConditions.Add(guard_condition);

            Assert.That(this.WaitSet.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.GuardConditions.Count, Is.EqualTo(1));
            Assert.That(this.WaitSet.GuardConditions, Does.Contain(guard_condition));
    
            Assert.That(this.WaitSet.GuardConditions.Remove(guard_condition), Is.True);

            Assert.That(this.WaitSet.GuardConditions, Does.Not.Contain(guard_condition));
        }

        [Test]
        public void TestTryWait()
        {
            this.Context.TryCreateNode("TestNode", out var node);

            using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC);
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { }
            );
            using var client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_TOPIC);
            using var service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_TOPIC,
                request => new AddTwoInts_Response()
            );

            this.WaitSet.Subscriptions.Add(subscription);
            this.WaitSet.Clients.Add(client);
            this.WaitSet.Services.Add(service);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);

            publisher.Publish(new std_msgs.msg.Int32());
            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.True);
            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.True);
            Assert.That(subscription.TryProcess(), Is.True);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);

            using var task = client.CallAsync(new AddTwoInts_Request());
            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.True);
            Assert.That(service.TryProcess(), Is.True);
            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.True);
            Assert.That(client.TryProcess(), Is.True);
            Assert.That(task.IsCompletedSuccessfully, Is.True);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);
        }

        [Test]
        public void TestTryWaitEmpty()
        {
            Assert.Throws<WaitSetEmptyException>(() => { this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _); });

            this.Context.TryCreateNode("TestNode", out var node);
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { throw new InvalidOperationException($"callback was triggered with {msg}"); }
            );
            this.WaitSet.Subscriptions.Add(subscription);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);

            Assert.That(this.WaitSet.Subscriptions.Remove(subscription), Is.True);
            Assert.Throws<WaitSetEmptyException>(() => { this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out _); });
        }

        [Test]
        public void TestResult()
        {
            this.Context.TryCreateNode("TestNode", out var node);

            using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC);
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { }
            );
            using var client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_TOPIC);
            using var service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_TOPIC,
                request => new AddTwoInts_Response()
            );
            this.WaitSet.Subscriptions.Add(subscription);
            this.WaitSet.Clients.Add(client);
            this.WaitSet.Services.Add(service);

            publisher.Publish(new std_msgs.msg.Int32());
            client.CallAsync(new AddTwoInts_Request());

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out var result1), Is.True);
            Assert.That(result1.ReadySubscriptions, Does.Contain(subscription).And.Exactly(1).Items);
            Assert.That(result1.ReadyServices, Does.Contain(service).And.Exactly(1).Items);
            Assert.That(result1, Has.Exactly(2).Items);

            Assert.That(subscription.TryProcess(), Is.True);
            Assert.That(service.TryProcess(), Is.True);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out var result2), Is.True);
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadySubscriptions.GetEnumerator().MoveNext(); });
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadyClients.GetEnumerator().MoveNext(); });
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadyServices.GetEnumerator().MoveNext(); });
            Assert.That(result2.ReadyClients, Does.Contain(client).And.Exactly(1).Items);
            Assert.That(result2, Has.Exactly(1).Items);

            this.WaitSet.Dispose();
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadySubscriptions.GetEnumerator().MoveNext(); });
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadyClients.GetEnumerator().MoveNext(); });
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadyServices.GetEnumerator().MoveNext(); });
        }

        [Test]
        public void TestResultMultiple()
        {
            this.Context.TryCreateNode("TestNode", out var node);
            using var publisher1 = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC + "1");
            using var publisher2 = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC + "2");
            using var publisher3 = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC + "3");
            using var subscription1 = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC + "1",
                msg => { }
            );
            using var subscription2 = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC + "2",
                msg => { }
            );
            using var subscription3 = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC + "3",
                msg => { }
            );
            this.WaitSet.Subscriptions.Add(subscription1);
            this.WaitSet.Subscriptions.Add(subscription2);
            this.WaitSet.Subscriptions.Add(subscription3);

            publisher1.Publish(new std_msgs.msg.Int32());
            publisher3.Publish(new std_msgs.msg.Int32());

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out var result1), Is.True);
            Assert.That(result1.ReadySubscriptions, Is.EquivalentTo(new ISubscriptionBase[]{ subscription1, subscription3 }));
            Assert.That(result1, Has.Exactly(2).Items);
        }
    }
}