using NUnit.Framework;
using System;
namespace ROS2.Test
{
    [TestFixture]
    public class CreateNodeTest
    {
        Context context;

        [SetUp]
        public void SetUp()
        {
            context = new Context();
            Ros2cs.Init(context);
        }

        [TearDown]
        public void TearDown()
        {
            Ros2cs.Shutdown(context);
        }

        [Test]
        public void CreateNode()
        {
            string nodeName = "create_node_test";
            Ros2cs.CreateNode(nodeName, context).Dispose();
        }

        [Test]
        public void CreateNodeWithNamespace()
        {
            string nodeName = "create_node_test";
            string nodeNamespace = "/ns";
            Node node = Ros2cs.CreateNode(nodeName, context, nodeNamespace: nodeNamespace);
            Assert.That(node.Namespace, Is.EqualTo("/ns"));
            node.Dispose();
        }

        [Test]
        public void CreateNodeWithEmptyNamespace()
        {
            string nodeName = "create_node_test";
            string nodeNamespace = "";
            Node node = Ros2cs.CreateNode(nodeName, context, nodeNamespace: nodeNamespace);
            Assert.That(node.Namespace, Is.EqualTo("/"));
            node.Dispose();
        }

        [Test]
        public void CreateNodeWithRelativeNamespace()
        {
            string nodeName = "create_node_test";
            string nodeNamespace = "ns";
            Node node = Ros2cs.CreateNode(nodeName, context, nodeNamespace: nodeNamespace);
            Assert.That(node.Namespace, Is.EqualTo("/ns"));
            node.Dispose();
        }

        [Test]
        public void CreateNodeWithInvalidName()
        {
            string nodeName = "create_node_test_invaild_name?";
            Assert.That( () => { Ros2cs.CreateNode(nodeName, context).DestroyNode(); },
                         Throws.TypeOf<InvalidNodeNameException>());
        }

        [Test]
        public void CreateNodeWithInvalidRelativeNamespace()
        {
            string nodeName = "create_node_test_invalid_namespace";
            string nodeNamespace = "invalid_namespace?";

            Assert.That( () => { Ros2cs.CreateNode(nodeName, context, nodeNamespace: nodeNamespace).DestroyNode(); },
                         Throws.TypeOf<InvalidNamespaceException>());
        }
    }
}
