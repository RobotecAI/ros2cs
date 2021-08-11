// Copyright 2019-2021 Robotec.ai
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

using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class ClockTest
    {
        [SetUp]
        public void SetUp()
        {
            Ros2cs.Init();
        }

        [TearDown]
        public void TearDown()
        {
            Ros2cs.Shutdown();
        }

        [Test]
        public void CreateClock()
        {
            Clock clock = new Clock();
        }

        [Test]
        public void ClockGetNow()
        {
            Clock clock = new Clock();
            RosTime timeNow = clock.Now;
            Assert.That(timeNow.sec, Is.Not.EqualTo(0));
        }

        [Test]
        public void RosTimeSeconds()
        {
            Clock clock = new Clock();

            RosTime oneSecond = new RosTime { sec = 1, nanosec = 0 };
            Assert.That(oneSecond.Seconds, Is.EqualTo(1.0d));

            RosTime twoPointSix = new RosTime { sec = 2, nanosec = 600000000 };
            Assert.That(twoPointSix.Seconds, Is.EqualTo(2.6d));
        }
    }
}
