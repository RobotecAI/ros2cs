// Copyright 2019-2023 Robotec.ai
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
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using example_interfaces.srv;

namespace ROS2.Test
{
    [TestFixture]
    public class ClientTest
    {
        private static readonly string SERVICE_NAME = "test_service";

        private Context Context;

        private INode Node;

        private IClient<AddTwoInts_Request, AddTwoInts_Response> Client;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
            Context.TryCreateNode("service_test_node", out Node);
            Client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_NAME);
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        private AddTwoInts_Request CreateRequest(int a, int b)
        {
            var msg = new AddTwoInts_Request();
            msg.A = a;
            msg.B = b;
            return msg;
        }

        private AddTwoInts_Response HandleRequest(AddTwoInts_Request msg)
        {
            var response = new AddTwoInts_Response();
            response.Sum = msg.A + msg.B;
            return response;
        }

        [Test]
        public void ClientCallAsync()
        {
            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            var task = Client.CallAsync(CreateRequest(42, 3));
            while (!task.IsCompleted)
            {
                service.TryProcess();
                Client.TryProcess();
            }
            Assert.That(task.Result.Sum, Is.EqualTo(45));
        }

        [Test]
        public void ClientCallAsyncConcurrent()
        {
            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            Task<AddTwoInts_Response>[] tasks = Enumerable
                .Range(0, 10)
                .Select(i => Client.CallAsync(CreateRequest(i, 100 - i)))
                .ToArray();
            while (!tasks.All(task => task.IsCompleted))
            {
                service.TryProcess();
                Client.TryProcess();
            }
            Assert.That(tasks.Select(task => task.Result.Sum), Is.All.EqualTo(100));
        }

        [Test]
        public void ClientWaitForService()
        {
            Assert.That(Client.IsServiceAvailable(), Is.False);
            {
                using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                    SERVICE_NAME,
                    HandleRequest
                );
                Assert.That(Client.IsServiceAvailable());
            }
            Assert.That(Client.IsServiceAvailable(), Is.False);
        }

        [Test]
        public void DisposedClientHandling()
        {
            Assert.That(Client.IsDisposed, Is.False);

            Client.Dispose();

            Assert.That(Client.IsDisposed);
            Assert.That(Node.Clients, Does.Not.Contain(Client));
            Assert.Throws<ObjectDisposedException>(() => Client.CallAsync(CreateRequest(42, 3)));
        }

        [Test]
        public void DoubleDisposeClient()
        {
            Client.Dispose();
            Client.Dispose();

            Assert.That(Client.IsDisposed);
        }

        [Test]
        public void DisposedClientTasks()
        {
            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            var task = Client.CallAsync(CreateRequest(42, 3));
            Client.Dispose();

            Assert.That(Client.IsDisposed);
            Assert.Throws<AggregateException>(task.Wait);
            Assert.That(task.IsFaulted);
            Assert.That(task.Exception.InnerExceptions.Any(e => e is ObjectDisposedException));
        }

        [Test]
        public void ClientPendingRequests()
        {
            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            Task[] tasks = Enumerable
                .Repeat(this.CreateRequest(3, 4), 3)
                .Select(request => this.Client.CallAsync(request))
                .ToArray();

            Assert.That(this.Client.PendingRequests.Count, Is.EqualTo(3));

            while (!tasks.Any(task => task.IsCompleted))
            {
                service.TryProcess();
                Client.TryProcess();
            }

            int completed = tasks.Where(task => task.IsCompletedSuccessfully).Count();

            Assert.That(completed, Is.GreaterThan(0));
            Assert.That(this.Client.PendingRequests.Count, Is.EqualTo(tasks.Length - completed));
        }

        [Test]
        public void ClientPendingRequestsCast()
        {
            IReadOnlyDictionary<long, Task> requests = (this.Client as IClientBase).PendingRequests;
            Task pendingTask = this.Client.CallAsync(this.CreateRequest(3, 4));

            Assert.That(requests.Keys, Is.EquivalentTo(this.Client.PendingRequests.Keys));
        }

        [Test]
        public void ClientCancel()
        {
            Assert.That(this.Client.Cancel(Task.CompletedTask), Is.False);

            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            Task finishedTask = this.Client.CallAsync(this.CreateRequest(3, 4));

            while (!finishedTask.IsCompleted)
            {
                service.TryProcess();
                Client.TryProcess();
            }

            Assert.That(this.Client.Cancel(finishedTask), Is.False);

            Task pendingTask = this.Client.CallAsync(this.CreateRequest(3, 4));

            Assert.That(this.Client.Cancel(pendingTask));
            Assert.That(pendingTask.IsCanceled);
            Assert.That(this.Client.PendingRequests.Count, Is.Zero);

            Assert.DoesNotThrow(() => { service.TryProcess(); Client.TryProcess(); });            
        }

        [Test]
        public void ClientTryProcess()
        {
            Assert.That(this.Client.TryProcess(), Is.False);

            using var service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                HandleRequest
            );
            Task pendingTask = this.Client.CallAsync(this.CreateRequest(3, 4));
            while (!service.TryProcess())
            {}
            while (!this.Client.TryProcess())
            {}

            Assert.That(pendingTask.IsCompletedSuccessfully);
        }
    }
}
