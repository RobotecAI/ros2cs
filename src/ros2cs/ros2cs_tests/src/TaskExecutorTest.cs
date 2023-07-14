using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ROS2.Executors;

namespace ROS2.Test
{
    [TestFixture]
    public class TaskExecutorTest
    {
        private readonly static string TOPIC = "/task_executor_test";

        private Context Context;

        private TaskExecutor Executor;

        [SetUp]
        public void SetUp()
        {
            this.Context = new Context();
            this.Executor = new TaskExecutor(this.Context, TimeSpan.FromSeconds(0.5));
        }

        [TearDown]
        public void TearDown()
        {
            this.Executor.Dispose();
            this.Context.Dispose();
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
        public void StopTask()
        {
            this.Executor.Dispose();

            Assert.Throws<AggregateException>(() => this.Executor.Task.Wait(TimeSpan.FromSeconds(1)));
            Assert.That(this.Executor.Task.Status, Is.EqualTo(TaskStatus.Canceled));
        }

        [Test]
        public void DisposeContext()
        {
            this.Context.Dispose();

            Assert.Throws<AggregateException>(() => this.Executor.Task.Wait(TimeSpan.FromSeconds(1)));
            Assert.That(this.Executor.Task.Status, Is.EqualTo(TaskStatus.Canceled));
        }


        [Test]
        public void ExceptionWhileSpinning()
        {
            if (this.Context.TryCreateNode("task_executor_test_node", out INode node))
            {
                using (node)
                {
                    this.Executor.Add(node);
                    using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(TOPIC);
                    using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(
                        TOPIC,
                        _ => { throw new Exception("simulated runtime exception"); }
                    );

                    publisher.Publish(new std_msgs.msg.Int32());

                    Assert.Throws<AggregateException>(() => this.Executor.Task.Wait(TimeSpan.FromSeconds(1)));
                    Assert.That(this.Executor.Task.Status, Is.EqualTo(TaskStatus.Faulted));

                    this.Executor.Dispose();
                    Assert.That(this.Executor.IsDisposed, Is.True);
                    Assert.That(node.Executor, Is.Null);
                }
            }
            else
            {
                throw new ArgumentException("node already exists");
            }
        }

        [Test]
        public void SpinningInBackground()
        {
            if (this.Context.TryCreateNode("task_executor_test_node", out INode node))
            {
                using (node)
                {
                    this.Executor.Add(node);
                    using var msgReceived = new ManualResetEventSlim(false);
                    using var publisher = node.CreatePublisher<std_msgs.msg.Int32>(TOPIC);
                    using var subscription = node.CreateSubscription<std_msgs.msg.Int32>(TOPIC, _ => msgReceived.Set());

                    publisher.Publish(new std_msgs.msg.Int32());

                    Assert.That(msgReceived.Wait(TimeSpan.FromSeconds(1)), Is.True);
                    Assert.That(this.Executor.Task.IsCompleted, Is.False);

                    this.Executor.Dispose();
                    Assert.That(this.Executor.IsDisposed, Is.True);
                    Assert.That(node.Executor, Is.Null);
                }
            }
            else
            {
                throw new ArgumentException("node already exists");
            }
        }
    }
}