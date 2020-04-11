using NUnit.Framework;
using System;
using System.Reflection;
using System.Text;
using rclcs;
using rclcs.Test;
using ROS2.Interfaces;
using System.Runtime.InteropServices;
using ROS2.Utils;

namespace rclcs.TestNativeMethods
{

    [TestFixture]
    public class RCLInitialize
    {
        public static void InitRcl(ref rcl_context_t context)
        {
            RCLReturnEnum ret;
            NativeMethods.rcl_reset_error();
            rcl_init_options_t init_options = NativeMethods.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            ret = (RCLReturnEnum)NativeMethods.rcl_init_options_init(ref init_options, allocator);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            context = NativeMethods.rcl_get_zero_initialized_context();

            ret = (RCLReturnEnum)NativeMethods.rcl_init(0, null, ref init_options, ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());

            Assert.That(NativeMethods.rcl_context_is_valid(ref context), Is.True);

        }

        public static void ShutdownRcl(ref rcl_context_t context)
        {
            RCLReturnEnum ret;
            ret = (RCLReturnEnum)NativeMethods.rcl_shutdown(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            ret = (RCLReturnEnum)NativeMethods.rcl_context_fini(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        [Test]
        public void InitShutdownFinalize()
        {
            rcl_context_t context = new rcl_context_t();
            InitRcl(ref context);
            ShutdownRcl(ref context);
        }
    }

    [TestFixture]
    public class RCL
    {
        rcl_context_t context = new rcl_context_t();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
        }

        [TearDown]
        public void TearDown()
        {
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void GetZeroInitializedContext()
        {
            rcl_context_t context = NativeMethods.rcl_get_zero_initialized_context();
        }

        [Test]
        public void GetDefaultAllocator()
        {
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
        }

        [Test]
        public void GetZeroInitializedInitOptions()
        {
            rcl_init_options_t init_options = NativeMethods.rcl_get_zero_initialized_init_options();
        }

        [Test]
        public void InitOptionsInit()
        {
            rcl_init_options_t init_options = NativeMethods.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            int ret = NativeMethods.rcl_init_options_init(ref init_options, allocator);
            Assert.That((RCLReturnEnum)ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        [Test]
        public void GetErrorString()
        {
            NativeMethods.rcl_reset_error();
            string message = Utils.GetRclErrorString();
            Assert.That(message, Is.EqualTo("error not set"));
        }

        [Test]
        public void ResetError()
        {
            NativeMethods.rcl_reset_error();
        }


        [Test]
        public void InitValidArgs()
        {
            rcl_init_options_t init_options = NativeMethods.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            NativeMethods.rcl_init_options_init(ref init_options, allocator);
            rcl_context_t context = NativeMethods.rcl_get_zero_initialized_context();

            RCLReturnEnum ret = (RCLReturnEnum)NativeMethods.rcl_init(2, new string[] { "foo", "bar" }, ref init_options, ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            Assert.That(NativeMethods.rcl_context_is_valid(ref context), Is.True);
            ret = (RCLReturnEnum)NativeMethods.rcl_shutdown(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            ret = (RCLReturnEnum)NativeMethods.rcl_context_fini(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

    }

    [TestFixture]
    public class NodeInitialize
    {
        rcl_context_t context = new rcl_context_t();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
        }

        [TearDown]
        public void TearDown()
        {
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void GetZeroInitializedNode()
        {
            rcl_node_t node = NativeMethods.rcl_get_zero_initialized_node();
        }

        [Test]
        public void NodeGetDefaultOptions()
        {
            IntPtr defaultNodeOptions = NativeMethods.rclcs_node_create_default_options();
            NativeMethods.rclcs_node_dispose_options(defaultNodeOptions);
        }

        public static void InitNode(ref rcl_node_t node, IntPtr nodeOptions, ref rcl_context_t context)
        {
            node = NativeMethods.rcl_get_zero_initialized_node();

            nodeOptions = NativeMethods.rclcs_node_create_default_options();
            string name = "node_test";
            string nodeNamespace = "/ns";

            RCLReturnEnum ret = (RCLReturnEnum)NativeMethods.rcl_node_init(ref node, name, nodeNamespace, ref context, nodeOptions);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        public static void ShutdownNode(ref rcl_node_t node, IntPtr nodeOptions)
        {
            NativeMethods.rcl_node_fini(ref node);
            NativeMethods.rclcs_node_dispose_options(nodeOptions);
        }

        [Test]
        public void NodeInitShutdown()
        {
            rcl_node_t node = new rcl_node_t();
            IntPtr nodeOptions = new IntPtr();

            InitNode(ref node, nodeOptions, ref context);
            ShutdownNode(ref node, nodeOptions);
        }

    }

    [TestFixture]
    public class Node
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void NodeGetNamespace()
        {
            string nodeNameFromRcl = MarshallingHelpers.PtrToString(NativeMethods.rcl_node_get_name(ref node));
            Assert.That("node_test", Is.EqualTo(nodeNameFromRcl));
        }

        [Test]
        public void NodeGetName()
        {
            string nodeNamespaceFromRcl = MarshallingHelpers.PtrToString(NativeMethods.rcl_node_get_namespace(ref node));
            Assert.That("/ns", Is.EqualTo(nodeNamespaceFromRcl));
        }

    }

    [TestFixture]
    public class PublisherInitialize
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void PublisherGetDefaultOptions()
        {
            rcl_publisher_options_t publisherOptions = NativeMethods.rcl_publisher_get_default_options();
        }

        [Test]
        public void GetZeroInitializedPublisher()
        {
            rcl_publisher_t publisher = NativeMethods.rcl_get_zero_initialized_publisher();
        }

        public static void InitPublisher(ref rcl_publisher_t publisher, ref rcl_node_t node, IntPtr publisherOptions)
        {
            RCLReturnEnum ret;
            publisher = NativeMethods.rcl_get_zero_initialized_publisher();
            publisherOptions = NativeMethods.rclcs_publisher_create_default_options();
            IMessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            ret = (RCLReturnEnum)NativeMethods.rcl_publisher_init(ref publisher, ref node, typeSupportHandle, "publisher_test_topic", publisherOptions);
        }

        public static void ShutdownPublisher(ref rcl_publisher_t publisher, ref rcl_node_t node, IntPtr publisherOptions)
        {
            RCLReturnEnum ret;
            ret = (RCLReturnEnum)NativeMethods.rcl_publisher_fini(ref publisher, ref node);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            NativeMethods.rclcs_publisher_dispose_options(publisherOptions);
        }

        [Test]
        public void PublisherInit()
        {
            rcl_publisher_t publisher = new rcl_publisher_t();
            IntPtr publisherOptions = new IntPtr();
            InitPublisher(ref publisher, ref node, publisherOptions);
            ShutdownPublisher(ref publisher, ref node, publisherOptions);

        }
    }

    [TestFixture]
    public class Publisher
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void PublisherPublish()
        {
            RCLReturnEnum ret;
            rcl_publisher_t publisher = new rcl_publisher_t();
            IntPtr publisherOptions = new IntPtr();
            PublisherInitialize.InitPublisher(ref publisher, ref node, publisherOptions);
            IMessageInternals msg = new std_msgs.msg.Bool();
            ret = (RCLReturnEnum)NativeMethods.rcl_publish(ref publisher, msg.Handle);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            PublisherInitialize.ShutdownPublisher(ref publisher, ref node, publisherOptions);

        }
    }

    [TestFixture]
    public class SubscriptionInitialize
    {
        [Test]
        public void GetZeroInitializedSubscription()
        {
            rcl_subscription_t subscription = NativeMethods.rcl_get_zero_initialized_subscription();
        }

        [Test]
        public void SubscriptionGetDefaultOptions()
        {
            IntPtr subscriptionOptions = NativeMethods.rclcs_subscription_create_default_options();
            NativeMethods.rclcs_subscription_dispose_options(subscriptionOptions);
        }

        public static void InitSubscription(ref rcl_subscription_t subscription, IntPtr subscriptionOptions, ref rcl_node_t node)
        {
            RCLReturnEnum ret;
            subscription = NativeMethods.rcl_get_zero_initialized_subscription();
            subscriptionOptions = NativeMethods.rclcs_subscription_create_default_options();
            IMessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            ret = (RCLReturnEnum)NativeMethods.rcl_subscription_init(ref subscription, ref node, typeSupportHandle, "/subscriber_test_topic", subscriptionOptions);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
        }

        public static void ShutdownSubscription(ref rcl_subscription_t subscription, IntPtr subscriptionOptions, ref rcl_node_t node)
        {
            RCLReturnEnum ret;
            ret = (RCLReturnEnum)NativeMethods.rcl_subscription_fini(ref subscription, ref node);
            NativeMethods.rclcs_subscription_dispose_options(subscriptionOptions);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
        }

        [Test]
        public void SubscriptionInit()
        {
            rcl_context_t context = new rcl_context_t();
            rcl_node_t node = new rcl_node_t();
            IntPtr nodeOptions = new IntPtr();

            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);

            rcl_subscription_t subscription = new rcl_subscription_t();
            IntPtr subscriptionOptions = new IntPtr();

            InitSubscription(ref subscription, subscriptionOptions, ref node);
            ShutdownSubscription(ref subscription, subscriptionOptions, ref node);

            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }
    }

    [TestFixture]
    public class Subscription
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();
        rcl_subscription_t subscription;
        IntPtr subscriptionOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
            SubscriptionInitialize.InitSubscription(ref subscription, subscriptionOptions, ref node);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }


        [Test]
        public void SubscriptionIsValid()
        {
            Assert.That(NativeMethods.rcl_subscription_is_valid(ref subscription), Is.True);
        }

        [Test]
        public void WaitSetAddSubscription()
        {
            NativeMethods.rcl_reset_error();

            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeMethods.rcl_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_init(ref waitSet, 1, 0, 0, 0, 0, 0, ref context, allocator));
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_clear(ref waitSet));

            Assert.That(NativeMethods.rcl_subscription_is_valid(ref subscription), Is.True);
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_add_subscription(ref waitSet, ref subscription, UIntPtr.Zero));

            RCLReturnEnum ret = (RCLReturnEnum)NativeMethods.rcl_wait(ref waitSet, Utils.TimeoutSecToNsec(0.01));
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_TIMEOUT));
        }
    }

    [TestFixture]
    public class WaitSet
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void GetZeroInitializedWaitSet()
        {
            // NOTE: The struct rcl_wait_set_t contains size_t
            // fields that are set to UIntPtr in C# declaration,
            // not guaranteed to work for all C implemenations/platforms.
            rcl_wait_set_t waitSet = NativeMethods.rcl_get_zero_initialized_wait_set();
        }

        [Test]
        public void WaitSetInit()
        {
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeMethods.rcl_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_init(ref waitSet, 1, 0, 0, 0, 0, 0, ref context, allocator));
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_fini(ref waitSet));
        }

        [Test]
        public void WaitSetClear()
        {
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeMethods.rcl_get_zero_initialized_wait_set();
            NativeMethods.rcl_wait_set_init(ref waitSet, 1, 0, 0, 0, 0, 0, ref context, allocator);
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_clear(ref waitSet));
            TestUtils.AssertRetOk(NativeMethods.rcl_wait_set_fini(ref waitSet));
        }

    }

    [TestFixture]
    public class QualityOfService
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void SetSubscriptionQosProfile()
        {
            rcl_subscription_t subscription = NativeMethods.rcl_get_zero_initialized_subscription();
            IntPtr subscriptionOptions = NativeMethods.rclcs_subscription_create_default_options();

            NativeMethods.rclcs_subscription_set_qos_profile(subscriptionOptions, 0);

            IMessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            NativeMethods.rcl_subscription_init(ref subscription, ref node, typeSupportHandle, "/subscriber_test_topic", subscriptionOptions);

            Assert.That(NativeMethods.rcl_subscription_is_valid(ref subscription), Is.True);

            NativeMethods.rcl_subscription_fini(ref subscription, ref node);
            NativeMethods.rclcs_subscription_dispose_options(subscriptionOptions);
        }


    }

    [TestFixture]
    public class Clock
    {
        rcl_context_t context;
        rcl_node_t node;
        IntPtr nodeOptions = new IntPtr();

        [SetUp]
        public void SetUp()
        {
            RCLInitialize.InitRcl(ref context);
            NodeInitialize.InitNode(ref node, nodeOptions, ref context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(ref node, nodeOptions);
            RCLInitialize.ShutdownRcl(ref context);
        }

        [Test]
        public void CreateClock()
        {
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            IntPtr clockHandle = NativeMethods.rclcs_ros_clock_create(ref allocator);
            NativeMethods.rclcs_ros_clock_dispose(clockHandle);
        }

        [Test]
        public void ClockGetNow()
        {
            rcl_allocator_t allocator = NativeMethods.rcutils_get_default_allocator();
            IntPtr clockHandle = NativeMethods.rclcs_ros_clock_create(ref allocator);
            long queryNow = 0;
            NativeMethods.rcl_clock_get_now(clockHandle, ref queryNow);

            Assert.That(queryNow, Is.Not.EqualTo(0));

            NativeMethods.rclcs_ros_clock_dispose(clockHandle);
        }

    }
}
