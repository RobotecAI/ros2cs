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

using NUnit.Framework;
using System;
using ROS2.Test;
using ROS2.Internal;

namespace ROS2.TestNativeMethods
{
    [TestFixture]
    public class RCLInitialize
    {
        public static void InitRcl(ref rcl_context_t context)
        {
            NativeRcl.rcl_reset_error();
            rcl_init_options_t init_options = NativeRcl.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            var ret = (RCLReturnEnum)NativeRcl.rcl_init_options_init(ref init_options, allocator);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            context = NativeRcl.rcl_get_zero_initialized_context();

            ret = (RCLReturnEnum)NativeRcl.rcl_init(0, null, ref init_options, ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            Assert.That(NativeRcl.rcl_context_is_valid(ref context), Is.True);
        }

        public static void ShutdownRcl(ref rcl_context_t context)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_shutdown(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            ret = (RCLReturnEnum)NativeRcl.rcl_context_fini(ref context);
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
            rcl_context_t context = NativeRcl.rcl_get_zero_initialized_context();
        }

        [Test]
        public void GetDefaultAllocator()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
        }

        [Test]
        public void GetZeroInitializedInitOptions()
        {
            rcl_init_options_t init_options = NativeRcl.rcl_get_zero_initialized_init_options();
        }

        [Test]
        public void InitOptionsInit()
        {
            rcl_init_options_t init_options = NativeRcl.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            int ret = NativeRcl.rcl_init_options_init(ref init_options, allocator);
            Assert.That((RCLReturnEnum)ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        [Test]
        public void GetErrorString()
        {
            NativeRcl.rcl_reset_error();
            string message = Utils.GetRclErrorString();
            Assert.That(message, Is.EqualTo("error not set"));
        }

        [Test]
        public void ResetError()
        {
            NativeRcl.rcl_reset_error();
        }

        [Test]
        public void InitValidArgs()
        {
            rcl_init_options_t init_options = NativeRcl.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            NativeRcl.rcl_init_options_init(ref init_options, allocator);
            rcl_context_t context = NativeRcl.rcl_get_zero_initialized_context();

            var ret = (RCLReturnEnum)NativeRcl.rcl_init(
                2, new string[] { "foo", "bar" }, ref init_options, ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            Assert.That(NativeRcl.rcl_context_is_valid(ref context), Is.True);
            ret = (RCLReturnEnum)NativeRcl.rcl_shutdown(ref context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            ret = (RCLReturnEnum)NativeRcl.rcl_context_fini(ref context);
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
            rcl_node_t node = NativeRcl.rcl_get_zero_initialized_node();
        }

        [Test]
        public void NodeGetDefaultOptions()
        {
            IntPtr defaultNodeOptions = NativeRclInterface.rclcs_node_create_default_options();
            NativeRclInterface.rclcs_node_dispose_options(defaultNodeOptions);
        }

        public static void InitNode(ref rcl_node_t node, IntPtr nodeOptions, ref rcl_context_t context)
        {
            node = NativeRcl.rcl_get_zero_initialized_node();

            nodeOptions = NativeRclInterface.rclcs_node_create_default_options();
            string name = "node_test";
            string nodeNamespace = "/ns";

            var ret = (RCLReturnEnum)NativeRcl.rcl_node_init(
                ref node, name, nodeNamespace, ref context, nodeOptions);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        public static void ShutdownNode(ref rcl_node_t node, IntPtr nodeOptions)
        {
            NativeRcl.rcl_node_fini(ref node);
            NativeRclInterface.rclcs_node_dispose_options(nodeOptions);
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
            string nodeNameFromRcl = Utils.PtrToString(NativeRcl.rcl_node_get_name(ref node));
            Assert.That("node_test", Is.EqualTo(nodeNameFromRcl));
        }

        [Test]
        public void NodeGetName()
        {
            string nodeNamespaceFromRcl = Utils.PtrToString(NativeRcl.rcl_node_get_namespace(ref node));
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
        public void PublisherCreateOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr publisherOptions = NativeRclInterface.rclcs_publisher_create_options(qos.handle);
        }

        [Test]
        public void GetZeroInitializedPublisher()
        {
            rcl_publisher_t publisher = NativeRcl.rcl_get_zero_initialized_publisher();
        }

        public static void InitPublisher(
            ref rcl_publisher_t publisher, ref rcl_node_t node, IntPtr publisherOptions)
        {
            publisher = NativeRcl.rcl_get_zero_initialized_publisher();
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            publisherOptions = NativeRclInterface.rclcs_publisher_create_options(qos.handle);
            MessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            var ret = (RCLReturnEnum)NativeRcl.rcl_publisher_init(
                ref publisher, ref node, typeSupportHandle, "publisher_test_topic", publisherOptions);
        }

        public static void ShutdownPublisher(
            ref rcl_publisher_t publisher, ref rcl_node_t node, IntPtr publisherOptions)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_publisher_fini(ref publisher, ref node);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            NativeRclInterface.rclcs_publisher_dispose_options(publisherOptions);
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
            rcl_publisher_t publisher = new rcl_publisher_t();
            IntPtr publisherOptions = new IntPtr();
            PublisherInitialize.InitPublisher(ref publisher, ref node, publisherOptions);
            MessageInternals msg = new std_msgs.msg.Bool();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();

            var ret = (RCLReturnEnum)NativeRcl.rcl_publish(ref publisher, msg.Handle, allocator.allocate);
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
            rcl_subscription_t subscription = NativeRcl.rcl_get_zero_initialized_subscription();
        }

        [Test]
        public void SubscriptionCreateOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qos.handle);
            NativeRclInterface.rclcs_subscription_dispose_options(subscriptionOptions);
        }

        public static void InitSubscription(
            ref rcl_subscription_t subscription, IntPtr subscriptionOptions, ref rcl_node_t node)
        {
            subscription = NativeRcl.rcl_get_zero_initialized_subscription();
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qos.handle);            
            MessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            var ret = (RCLReturnEnum)NativeRcl.rcl_subscription_init(
                ref subscription, ref node, typeSupportHandle, "/subscriber_test_topic", subscriptionOptions);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
        }

        public static void ShutdownSubscription(
            ref rcl_subscription_t subscription, IntPtr subscriptionOptions, ref rcl_node_t node)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_subscription_fini(ref subscription, ref node);
            NativeRclInterface.rclcs_subscription_dispose_options(subscriptionOptions);
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
            Assert.That(NativeRcl.rcl_subscription_is_valid(ref subscription), Is.True);
        }

        [Test]
        public void WaitSetAddSubscription()
        {
            NativeRcl.rcl_reset_error();

            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeRcl.rcl_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_init(
                ref waitSet,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                ref context,
                allocator
            ));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_clear(ref waitSet));

            Assert.That(NativeRcl.rcl_subscription_is_valid(ref subscription), Is.True);
            UIntPtr index = (UIntPtr)42;
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_add_subscription(ref waitSet, ref subscription, ref index));
            Assert.That(index.ToUInt64(), Is.EqualTo(0));

            long timeout_ns = 10*1000*1000;
            var ret = (RCLReturnEnum)NativeRcl.rcl_wait(ref waitSet, timeout_ns);
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
            rcl_wait_set_t waitSet = NativeRcl.rcl_get_zero_initialized_wait_set();
        }

        [Test]
        public void WaitSetInit()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeRcl.rcl_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_init(
                ref waitSet,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                ref context,
                allocator
            ));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_fini(ref waitSet));
        }

        [Test]
        public void WaitSetClear()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            rcl_wait_set_t waitSet = NativeRcl.rcl_get_zero_initialized_wait_set();
            NativeRcl.rcl_wait_set_init(
                ref waitSet,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                ref context,
                allocator
            );
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_clear(ref waitSet));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_fini(ref waitSet));
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
            rcl_subscription_t subscription = NativeRcl.rcl_get_zero_initialized_subscription();

            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qos.handle);

            MessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            NativeRcl.rcl_subscription_init(
                ref subscription, ref node, typeSupportHandle, "/subscriber_test_topic", subscriptionOptions);

            Assert.That(NativeRcl.rcl_subscription_is_valid(ref subscription), Is.True);

            NativeRcl.rcl_subscription_fini(ref subscription, ref node);
            NativeRclInterface.rclcs_subscription_dispose_options(subscriptionOptions);
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
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            IntPtr clockHandle = NativeRclInterface.rclcs_ros_clock_create(ref allocator);
            NativeRclInterface.rclcs_ros_clock_dispose(clockHandle);
        }

        [Test]
        public void ClockGetNow()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            IntPtr clockHandle = NativeRclInterface.rclcs_ros_clock_create(ref allocator);
            long queryNow = 0;
            NativeRcl.rcl_clock_get_now(clockHandle, ref queryNow);

            Assert.That(queryNow, Is.Not.EqualTo(0));

            NativeRclInterface.rclcs_ros_clock_dispose(clockHandle);
        }
    }
}
