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
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using example_interfaces.srv;

namespace ROS2.Test
{
    [TestFixture]
    public class ClientTest
    {
        private static readonly string SERVICE_NAME = "test_service";

        private INode Node;

        private IClient<AddTwoInts_Request, AddTwoInts_Response> Client;

        [SetUp]
        public void SetUp()
        {
            Ros2cs.Init();
            Node = Ros2cs.CreateNode("service_test_node");
            Client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_NAME);
        }

        [TearDown]
        public void TearDown()
        {
            Node.Dispose();
            Ros2cs.Shutdown();
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
                Ros2cs.SpinOnce(Node, 0.1);
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
                Ros2cs.SpinOnce(Node, 0.1);
            }
            Assert.That(tasks.Select(task => task.Result.Sum), Is.All.EqualTo(100));
        }

        [Test]
        public void DisposedClientHandling()
        {
            Assert.That(!Client.IsDisposed);
            Client.Dispose();
            Assert.That(Client.IsDisposed);
            Assert.DoesNotThrow(() => { Ros2cs.SpinOnce(Node, 0.1); });
        }

        [Test]
        public void DisposedClientTasks()
        {
            Ros2cs.SpinOnce(Node, 0.1);
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
            Assert.DoesNotThrow(() => { Ros2cs.SpinOnce(Node, 0.1); });
        }

        [Test]
        public void ReinitializeDisposedClient()
        {
            Client.Dispose();
            Client = Node.CreateClient<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_NAME);
            Assert.DoesNotThrow(() => { Ros2cs.SpinOnce(Node, 0.1); });
        }
    }
}
