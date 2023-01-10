using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ROS2.Test
{
    [TestFixture]
    public class ContextTest
    {
        private Context Context;

        [SetUp]
        public void SetUp()
        {
            Context = new Context();
        }

        [TearDown]
        public void TearDown()
        {
            Context.Dispose();
        }

        [Test]
        public void ContextOk()
        {
            Assert.That(Context.Ok());
            Assert.That(Context.IsDisposed, Is.False);

            Context.Dispose();

            Assert.That(Context.Ok(), Is.False);
            Assert.That(Context.IsDisposed);
        }

        [Test]
        public void ContextOnShutdown()
        {
            int called = 0;
            Context.OnShutdown += () => { called += 1; };

            Context.Dispose();

            Assert.That(called, Is.EqualTo(1));

            Context.Dispose();

            Assert.That(called, Is.EqualTo(1));
        }

        [Test]
        public void MultipleContexts()
        {
            using (Context secondContext = new Context())
            {
                Assert.That(Context.IsDisposed, Is.False);
            }
            Assert.That(Context.IsDisposed, Is.False);
        }

        [Test]
        public void ContextDoubleDisposal()
        {
            Context.Dispose();
            Context.Dispose();

            Assert.That(Context.IsDisposed);
        }

        [Test]
        public void ContextCreateNode()
        {
            string name = "test";
            var nodes = Context.Nodes;

            Assert.That(Context.TryCreateNode(name, out INode node));
            Assert.That(nodes, Contains.Item(new KeyValuePair<string, INode>(name, node)));
            Assert.That(node.Name, Is.EqualTo(name));
        }

        [Test]
        public void ContextRecreateNode()
        {
            string name = "test";
            Assert.That(Context.TryCreateNode(name, out INode node));
            node.Dispose();

            Assert.That(Context.TryCreateNode(name, out _));
        }

        [Test]
        public void ContextCreateNodeDisposed()
        {
            Context.Dispose();

            Assert.That(() => { Context.TryCreateNode("test", out _); }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void ContextRejectDuplicateNode()
        {
            string name = "test";
            Assert.That(Context.TryCreateNode(name, out _));

            Assert.That(Context.TryCreateNode(name, out _), Is.False);
            Assert.That(Context.Nodes.Count, Is.EqualTo(1));
            Assert.That(Context.TryCreateNode(name + "2", out _));
        }

        [Test]
        public void ContextKeepAliveNode()
        {
            Assert.That(Context.TryCreateNode("test", out INode node));
            var weakRef = new WeakReference<INode>(node);
            node = null;
            GC.Collect();

            Assert.That(weakRef.TryGetTarget(out _));
        }

        [Test]
        public void ContextDisposeNode()
        {
            Assert.That(Context.TryCreateNode("test", out INode node));

            Assert.That(Context.Nodes.Values, Contains.Item(node));

            node.Dispose();

            Assert.That(Context.Nodes.Values, Does.Not.Contain(node));
        }

        [Test]
        public void ContextDisposeNodes()
        {
            Assert.That(Context.TryCreateNode("test1", out INode node1));
            Assert.That(Context.TryCreateNode("test2", out INode node2));

            Context.Dispose();

            Assert.That(node1.IsDisposed);
            Assert.That(node2.IsDisposed);
            Assert.That(Context.Nodes.Values, Is.Empty);
        }
    }
}
