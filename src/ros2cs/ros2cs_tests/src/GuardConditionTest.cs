// Copyright 2023 ADVITEC Informatik GmbH - www.advitec.de
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
using System.Threading;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class GuardConditionTest
    {
        private Context Context;

        private GuardCondition GuardCondition;

        [SetUp]
        public void SetUp()
        {
            this.Context = new Context();
            this.GuardCondition = this.Context.CreateGuardCondition(
                () => { throw new InvalidOperationException("guard condition was called"); }
            );
        }

        [TearDown]
        public void TearDown()
        {
            this.Context.Dispose();
        }

        [Test]
        public void DisposedGuardConditionHandling()
        {
            Assert.That(this.GuardCondition.IsDisposed, Is.False);

            this.Context.Dispose();

            Assert.That(this.GuardCondition.IsDisposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => this.GuardCondition.Trigger());
        }

        [Test]
        public void DoubleDisposeGuardCondition()
        {
            this.GuardCondition.Dispose();
            this.GuardCondition.Dispose();

            Assert.That(this.GuardCondition.IsDisposed, Is.True);
        }

        [Test]
        public void OnShutdownDisposal()
        {
            this.Context.OnShutdown += () => Assert.That(this.GuardCondition.IsDisposed, Is.False);

            this.Context.Dispose();
        }

        [Test]
        public void DisposedContextHandling()
        {
            this.Context.Dispose();

            Assert.Throws<ObjectDisposedException>(() => this.Context.CreateGuardCondition(() => { }));
        }

        [Test]
        public void TriggerGuardCondition()
        {
            using var waitSet = new WaitSet(this.Context);
            waitSet.GuardConditions.Add(this.GuardCondition);

            Assert.That(waitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);

            using var timer = new Timer(
                _ => this.GuardCondition.Trigger(),
                null,
                TimeSpan.FromSeconds(0.25),
                Timeout.InfiniteTimeSpan
            );
    
            Assert.That(waitSet.TryWait(TimeSpan.FromSeconds(0.5), out var result), Is.True);
            Assert.That(result.ReadyGuardConditions, Does.Contain(this.GuardCondition));
        }

        [Test]
        public void TriggerGuardConditionNotWaiting()
        {
            using var waitSet = new WaitSet(this.Context);
            waitSet.GuardConditions.Add(this.GuardCondition);

            this.GuardCondition.Trigger();

            Assert.That(waitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.True);
            Assert.That(waitSet.TryWait(TimeSpan.FromSeconds(0.1), out _), Is.False);
        }
    }
}