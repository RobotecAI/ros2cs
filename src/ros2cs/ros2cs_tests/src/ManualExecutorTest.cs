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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ROS2.Executors;

namespace ROS2.Test
{
    [TestFixture]
    public class ManualExecutorTest
    {
        private static readonly string SUBSCRIPTION_TOPIC = "test_executor";

        private Context Context;

        private ManualExecutor Executor;

        private WaitSet WaitSet;

        [SetUp]
        public void SetUp()
        {
            this.Context = new Context();
            this.WaitSet = new WaitSet(this.Context);
            this.Executor = new ManualExecutor(
                this.WaitSet,
                new GuardCondition(this.Context, () => { })
            );
        }

        [TearDown]
        public void TearDown()
        {
            this.Executor.Dispose();
            this.Context.Dispose();
        }

        [Test]
        public void DisposedExecutorHandling()
        {
            Assert.That(this.Executor.IsDisposed, Is.False);

            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);
            this.Executor.Rescan();
            this.Executor.Dispose();

            Assert.That(this.Executor.IsDisposed, Is.True);
            Assert.That(node.Executor, Is.Null);
            Assert.That(this.Executor, Does.Not.Contain(node));
            Assert.Throws<ObjectDisposedException>(() => { this.Executor.TrySpin(TimeSpan.FromSeconds(0.1)); });
        }

        [Test]
        public void DoubleDisposeExecutor()
        {
            Assert.That(this.Executor.IsDisposed, Is.False);

            this.Executor.Dispose();
            this.Executor.Dispose();

            Assert.That(this.Executor.IsDisposed, Is.True);
        }

        [Test]
        public void RemoveNodeAfterContextDispose()
        {
            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);

            this.Context.Dispose();

            Assert.That(node.Executor, Is.Null);
            Assert.That(this.Executor, Does.Not.Contain(node));
            Assert.That(node.IsDisposed, Is.True);
        }

        [Test]
        public void ScheduleRescan()
        {
            Assert.That(this.Executor.RescanScheduled, Is.False);

            this.Executor.ScheduleRescan();

            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void RescheduleRescan()
        {
            Assert.That(this.Executor.RescanScheduled, Is.False);

            this.Executor.ScheduleRescan();
            this.Executor.ScheduleRescan();

            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void Wait()
        {
            this.Executor.Wait();
            this.Executor.ScheduleRescan();
            this.Executor.Wait();
        }

        [Test]
        public void TryWaitScheduled()
        {
            this.Executor.ScheduleRescan();

            Assert.That(this.Executor.TryWait(TimeSpan.Zero), Is.True);
            this.Executor.Wait();
            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void TryWaitUnscheduled()
        {
            Assert.That(this.Executor.TryWait(TimeSpan.Zero), Is.True);
            this.Executor.Wait();
            Assert.That(this.Executor.RescanScheduled, Is.False);
        }

        [Test]
        public void TryWaitInterrupt()
        {
            Thread spinThread = new Thread(() => { this.Executor.TrySpin(TimeSpan.FromMinutes(1)); });
            spinThread.Start();
            // wait for spin to start
            while (!this.Executor.IsSpinning)
            { }
            this.Executor.ScheduleRescan();

            Assert.That(this.Executor.TryWait(TimeSpan.FromSeconds(10)), Is.True);
            Assert.That(spinThread.Join(TimeSpan.FromSeconds(10)), Is.True);
        }

        [Test]
        public void TryWaitTimeout()
        {
            using ManualResetEventSlim isTriggered = new ManualResetEventSlim(false);
            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);
            using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC
            );
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg =>
                {
                    isTriggered.Set();
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            );
            this.Executor.Rescan();
            Thread spinThread = new Thread(() =>
            {
                this.Executor.TrySpin(TimeSpan.FromMinutes(0.5));
            });
            spinThread.Start();
            publisher.Publish(new std_msgs.msg.Int32());
            isTriggered.Wait(TimeSpan.FromSeconds(1));
            this.Executor.ScheduleRescan();

            Assert.That(this.Executor.TryWait(TimeSpan.FromSeconds(0.5)), Is.False);
            Assert.That(spinThread.Join(TimeSpan.FromSeconds(10)), Is.True);
        }

        [Test]
        public void InterruptWaiting()
        {
            Thread spinThread = new Thread(() => { this.Executor.TrySpin(TimeSpan.FromMinutes(1)); });
            spinThread.Start();
            // wait for spin to start
            while (!this.Executor.IsSpinning)
            { }

            this.Executor.Interrupt();

            Assert.That(spinThread.Join(TimeSpan.FromSeconds(10)), Is.True);
        }

        [Test]
        public void InterruptNotWaiting()
        {
            Assert.That(this.Executor.IsSpinning, Is.False);

            this.Executor.Interrupt();
        }

        [Test]
        public void TrySpin()
        {
            std_msgs.msg.Int32 received = null;
            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);
            using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC
            );
            using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                SUBSCRIPTION_TOPIC,
                msg => { received = msg; }
            );
            this.Executor.Rescan();

            Assert.That(this.Executor.TrySpin(TimeSpan.FromSeconds(0.1)), Is.True);
            Assert.That(received, Is.Null);

            publisher.Publish(new std_msgs.msg.Int32());

            Assert.That(this.Executor.TrySpin(TimeSpan.FromSeconds(0.1)), Is.True);
            Assert.That(received, Is.Not.Null);
        }

        [Test]
        public void TrySpinRescanScheduled()
        {
            this.Executor.ScheduleRescan();
            Assert.That(this.Executor.TrySpin(TimeSpan.FromSeconds(0)), Is.False);
        }

        [Test]
        public void TrySpinEmpty()
        {
            Assert.That(this.Executor.TrySpin(TimeSpan.FromSeconds(0)), Is.True);
        }

        [Test]
        public void Rescan()
        {
            this.Executor.ScheduleRescan();
            this.Executor.Rescan();

            Assert.That(this.Executor.RescanScheduled, Is.False);
        }

        [Test]
        public void RescanEmpty()
        {
            this.Executor.Rescan();
            Assert.That(this.Executor.RescanScheduled, Is.False);
        }

        [Test]
        public void AddNoExecutor()
        {
            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);

            Assert.That(node.Executor, Is.SameAs(this.Executor));
            Assert.That(this.Executor, Does.Contain(node));
            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void AddExecutor()
        {
            this.Context.TryCreateNode("test_node", out var node);

            node.Executor = new DummyExecutor();

            Assert.Throws<InvalidOperationException>(() => { this.Executor.Add(node); });
            Assert.That(node.Executor, Is.Not.SameAs(this.Executor));
            Assert.That(this.Executor, Does.Not.Contain(node));
            Assert.That(this.Executor.RescanScheduled, Is.False);
        }

        [Test]
        public void RemoveContains()
        {
            this.Context.TryCreateNode("test_node", out var node);
            this.Executor.Add(node);
            this.Executor.Rescan();

            Assert.That(this.Executor.Remove(node), Is.True);
            Assert.That(node.Executor, Is.Null);
            Assert.That(this.Executor, Does.Not.Contain(node));
            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void RemoveNotContains()
        {
            this.Context.TryCreateNode("test_node", out var node);

            Assert.That(this.Executor.Remove(node), Is.False);
            Assert.That(this.Executor.RescanScheduled, Is.False);
        }

        [Test]
        public void Clear()
        {
            this.Context.TryCreateNode("test_node1", out var node1);
            this.Executor.Add(node1);
            this.Context.TryCreateNode("test_node2", out var node2);
            this.Executor.Add(node2);
            this.Executor.Rescan();

            this.Executor.Clear();

            Assert.That(node1.Executor, Is.Null);
            Assert.That(node2.Executor, Is.Null);
            Assert.That(this.Executor, Does.Not.Contain(node1));
            Assert.That(this.Executor, Does.Not.Contain(node2));
            Assert.That(this.Executor.Count, Is.Zero);
            Assert.That(this.Executor.RescanScheduled, Is.True);
        }

        [Test]
        public void SpinInTask()
        {
            using ManualResetEventSlim wasSpun = new ManualResetEventSlim(false);
            using var guardCondition = this.Context.CreateGuardCondition(wasSpun.Set);
            this.WaitSet.GuardConditions.Add(guardCondition);

            using var cancellationSource = new CancellationTokenSource();
            using Task spinTask = this.Executor.CreateSpinTask(TimeSpan.FromSeconds(0.5), cancellationSource.Token);

            Assert.That(spinTask.Status, Is.EqualTo(TaskStatus.Created));

            spinTask.Start();
            try
            {
                while (spinTask.Status != TaskStatus.Running)
                {
                    Thread.Yield(); // wait for task to be scheduled
                }
                Assert.That(wasSpun.Wait(TimeSpan.FromSeconds(1)), Is.False);
                guardCondition.Trigger();
                Assert.That(wasSpun.Wait(TimeSpan.FromSeconds(1)), Is.True);
                wasSpun.Reset();
            }
            finally
            {
                cancellationSource.Cancel();
                try
                {
                    spinTask.Wait();
                }
                catch (AggregateException e)
                {
                    e.Handle(inner => inner is TaskCanceledException);
                }
            }

            Assert.That(spinTask.Status, Is.EqualTo(TaskStatus.Canceled));
            guardCondition.Trigger();
            Assert.That(wasSpun.Wait(TimeSpan.FromSeconds(1)), Is.False);
        }

        [Test]
        public void ExceptionWhileSpinningInTask()
        {
            using var guardCondition = this.Context.CreateGuardCondition(() =>
            {
                throw new SimulatedException("simulating runtime exception");
            });
            this.WaitSet.GuardConditions.Add(guardCondition);

            using var cancellationSource = new CancellationTokenSource();
            using Task spinTask = this.Executor.CreateSpinTask(TimeSpan.FromSeconds(0.5), cancellationSource.Token);

            spinTask.Start();
            try
            {
                while (spinTask.Status != TaskStatus.Running)
                {
                    Thread.Yield(); // wait for task to be scheduled
                }
                guardCondition.Trigger();
                var exception = Assert.Throws<AggregateException>(() => spinTask.Wait(TimeSpan.FromSeconds(1)));
                Assert.That(exception.InnerExceptions, Has.Some.Matches(new Predicate<Exception>(e => e is SimulatedException)));
                Assert.That(spinTask.Status, Is.EqualTo(TaskStatus.Faulted));
            }
            finally
            {
                cancellationSource.Cancel();
                try
                {
                    spinTask.Wait();
                }
                catch (AggregateException e)
                {
                    e.Handle(inner => inner is TaskCanceledException || inner is SimulatedException);
                }
            }
        }

        private sealed class SimulatedException : Exception
        {
            public SimulatedException(string msg) : base(msg)
            { }
        }

        private sealed class DummyExecutor : HashSet<INode>, IExecutor
        {
            public bool IsDisposed
            {
                get { return false; }
            }

            public void ScheduleRescan()
            { }

            public bool TryScheduleRescan(INode node)
            {
                return true;
            }

            public void Wait()
            { }

            public bool TryWait(TimeSpan timeout)
            {
                return true;
            }

            public void Dispose()
            { }
        }
    }
}