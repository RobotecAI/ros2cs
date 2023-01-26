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

        private INode Node;

        private IService<AddTwoInts_Request, AddTwoInts_Response> Service;

        private Func<AddTwoInts_Request, AddTwoInts_Response> OnRequest =
            msg => throw new InvalidOperationException("callback not set");

        [SetUp]
        public void SetUp()
        {
            Ros2cs.Init();
            Node = Ros2cs.CreateNode("service_test_node");
            Service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_NAME, OnRequest);
        }

        [TearDown]
        public void TearDown()
        {
            Node.Dispose();
            Ros2cs.Shutdown();
        }

        [Test]
        public void DisposedServiceHandling()
        {
            Assert.That(!Service.IsDisposed);
            Service.Dispose();
            Assert.That(Service.IsDisposed);
            Assert.DoesNotThrow(() => { Ros2cs.SpinOnce(Node, 0.1); });
        }

        [Test]
        public void ReinitializeDisposedService()
        {
            Service.Dispose();
            Service = Node.CreateService<AddTwoInts_Request, AddTwoInts_Response>(SERVICE_NAME, OnRequest);
            Assert.DoesNotThrow(() => { Ros2cs.SpinOnce(Node, 0.1); });
        }
    }
}
