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
            this.GuardCondition = new GuardCondition(
                this.Context,
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
        public void DisposedContextHandling()
        {
            this.Context.Dispose();

            Assert.Throws<ObjectDisposedException>(() => new GuardCondition(this.Context, () => {}));
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
            Assert.That(result.ReadyGuardConditions.Values.Contains(this.GuardCondition), Is.True);
        }
    }
}