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
        public void TestReadyDictionary()
        {
            this.Context.TryCreateNode("TestNode", out var node);

            using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(SUBSCRIPTION_TOPIC);
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { }
            );
            using var subscriptionDummy = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { throw new InvalidOperationException($"callback was triggered with {msg}"); }
            );
            using var client = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_TOPIC);
            using var clientDummy = node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_TOPIC);
            using var service = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_TOPIC,
                request => new AddTwoInts_Response()
            );
            using var serviceDummy = node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_TOPIC,
                request => { throw new InvalidOperationException($"received request ${request}"); }
            );

            this.WaitSet.Subscriptions.Add(subscription);
            this.WaitSet.Clients.Add(client);
            this.WaitSet.Services.Add(service);

            publisher.Publish(new std_msgs.msg.Int32());
            client.CallAsync(new AddTwoInts_Request());

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out var result1), Is.True);
            this.TestReadyDictionary(result1.ReadySubscriptions, subscription, subscriptionDummy);
            this.TestReadyDictionary(result1.ReadyServices, service, serviceDummy);
            Assert.That(result1.ReadyClients.Count, Is.Zero);

            Assert.That(subscription.TryProcess(), Is.True);
            Assert.That(service.TryProcess(), Is.True);

            Assert.That(this.WaitSet.TryWait(TimeSpan.FromSeconds(0.1), out var result2), Is.True);
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadySubscriptions.Count; });
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadyClients.Count; });
            Assert.Catch<InvalidOperationException>(() => { _ = result1.ReadyServices.Count; });
            Assert.That(result2.ReadySubscriptions.Count, Is.Zero);
            Assert.That(result2.ReadyServices.Count, Is.Zero);
            this.TestReadyDictionary(result2.ReadyClients, client, clientDummy);

            this.WaitSet.Dispose();
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadySubscriptions.Count; });
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadyClients.Count; });
            Assert.Catch<InvalidOperationException>(() => { _ = result2.ReadyServices.Count; });
        }

        private void TestReadyDictionary<T>(IDictionary<int, T> dictionary, T waitable, T dummy) where T : IWaitable
        {
            Assert.That(dictionary.Count, Is.EqualTo(1));
            Assert.That(dictionary.Count(), Is.EqualTo(1));

            Assert.Throws<NotSupportedException>(() => { dictionary[1] = waitable; });
            Assert.Throws<NotSupportedException>(() => { dictionary.Add(1, waitable); });

            var pair = dictionary.First();
            Assert.That(pair.Value, Is.EqualTo(waitable));

            Assert.That(dictionary.Contains(new KeyValuePair<int, T>(pair.Key, waitable)), Is.True);
            Assert.That(dictionary.Contains(new KeyValuePair<int, T>(pair.Key + 1, waitable)), Is.False);
            Assert.That(dictionary.Contains(new KeyValuePair<int, T>(pair.Key, dummy)), Is.False);

            Assert.That(dictionary.ContainsKey(pair.Key), Is.True);
            Assert.That(dictionary.ContainsKey(pair.Key + 1), Is.False);

            Assert.That(dictionary.TryGetValue(pair.Key, out var result), Is.True);
            Assert.That(result, Is.EqualTo(waitable));
            Assert.That(dictionary.TryGetValue(pair.Key + 1, out _), Is.False);

            this.TestReadyDictionaryKeys(dictionary.Keys, pair.Key);
            this.TestReadyDictionaryValues(dictionary.Values, waitable, dummy);

            Assert.That(dictionary.Remove(pair.Key + 1), Is.False);
            Assert.That(dictionary.Remove(pair), Is.True);
            Assert.That(dictionary.Remove(pair), Is.False);
            Assert.That(dictionary.Count, Is.EqualTo(0));
        }

        private void TestReadyDictionaryKeys(ICollection<int> keys, int key)
        {
            Assert.That(keys.Count, Is.EqualTo(1));
            Assert.That(keys.Count(), Is.EqualTo(1));

            Assert.Throws<NotSupportedException>(() => { keys.Add(key + 1); });

            Assert.That(keys.Contains(key), Is.True);
            Assert.That(keys.Contains(key + 1), Is.False);

            Assert.That(keys.First(), Is.EqualTo(key));
        }

        private void TestReadyDictionaryValues<T>(ICollection<T> values, T value, T dummy) where T : IWaitable
        {
            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.Count(), Is.EqualTo(1));

            Assert.Throws<NotSupportedException>(() => { values.Add(value); });

            Assert.That(values.Contains(value), Is.True);
            Assert.That(values.Contains(dummy), Is.False);

            Assert.That(values.First(), Is.EqualTo(value));
        }
    }
}