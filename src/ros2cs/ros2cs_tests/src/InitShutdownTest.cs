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

namespace ROS2.Test
{
    [TestFixture]
    public class InitShutdownTest
    {
        [Test]
        public void Init()
        {
            Ros2cs.Init();
            try
            {
                Ros2cs.Shutdown();
            }
            catch (RuntimeError)
            {
            }
        }

        [Test]
        public void InitShutdown()
        {
            Ros2cs.Init();
            Ros2cs.Shutdown();
        }

        [Test]
        public void InitShutdownSequence()
        {
            Ros2cs.Init();
            Ros2cs.Shutdown();
            Ros2cs.Init();
            Ros2cs.Shutdown();
        }

        [Test]
        public void DoubleInit()
        {
            Ros2cs.Init();
            Ros2cs.Init();
            Ros2cs.Shutdown();
        }

        [Test]
        public void DoubleShutdown()
        {
            Ros2cs.Init();
            Ros2cs.Shutdown();
            Ros2cs.Shutdown();
        }

        [Test]
        public void CreateNodeWithoutInit()
        {
            Assert.That(() => { Ros2cs.CreateNode("foo"); }, Throws.TypeOf<NotInitializedException>());
        }

        [Test]
        public void SpinEmptyNode()
        {
            Ros2cs.Init();
            try
            {
                var node = Ros2cs.CreateNode("TestNode");
                Assert.That(Ros2cs.SpinOnce(node), Is.False);
                var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                    "subscription_test_topic",
                    (msg) => { throw new InvalidOperationException("subscription callback was triggered"); }
                );
                Assert.That(Ros2cs.SpinOnce(node), Is.True);
            }
            finally
            {
                Ros2cs.Shutdown();
            }
        }
    }
}
