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
    public class ServiceTest
    {
        private static readonly string SERVICE_NAME = "test_service";

        private Context Context;

        private INode Node;

        private IService<AddTwoInts_Request, AddTwoInts_Response> Service;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
            Context.TryCreateNode("service_test_node", out Node);
            Service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(
                SERVICE_NAME,
                request => { throw new InvalidOperationException($"received request ${request}"); }
            );
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        [Test]
        public void DisposedServiceHandling()
        {
            Assert.That(Service.IsDisposed, Is.False);

            Service.Dispose();

            Assert.That(Service.IsDisposed);
            Assert.That(Node.Services, Does.Not.Contain(Service));
        }

        [Test]
        public void DoubleDisposeService()
        {
            Service.Dispose();
            Service.Dispose();

            Assert.That(Service.IsDisposed);
        }

        [Test]
        public void ServiceTryProcess()
        {
            Assert.That(Service.TryProcess(), Is.False);
        }
    }
}
