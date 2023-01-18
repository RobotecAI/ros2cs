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
using ROS2.Internal;
using ROS2.Test;

namespace ROS2.TestNativeMethods
{
    [TestFixture]
    public class RCLInitialize
    {
        internal static IntPtr InitRcl()
        {
            NativeRcl.rcl_reset_error();
            rcl_init_options_t init_options = NativeRcl.rcl_get_zero_initialized_init_options();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            var ret = (RCLReturnEnum)NativeRcl.rcl_init_options_init(ref init_options, allocator);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            IntPtr context = NativeRclInterface.rclcs_get_zero_initialized_context();
            ret = (RCLReturnEnum)NativeRcl.rcl_init(0, null, ref init_options, context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            Assert.That(NativeRclInterface.rclcs_context_is_valid(context), Is.True);
            return context;
        }

        internal static void ShutdownRcl(IntPtr context)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_shutdown(context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
            Assert.That(NativeRclInterface.rclcs_context_is_valid(context), Is.False);

            ret = (RCLReturnEnum)NativeRcl.rcl_context_fini(context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));

            NativeRclInterface.rclcs_free_context(context);
        }

        [Test]
        public void InitShutdownFinalize()
        {
            var context = InitRcl();
            ShutdownRcl(context);
        }
    }

    [TestFixture]
    public class RCL
    {
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
            IntPtr context = NativeRclInterface.rclcs_get_zero_initialized_context();

            var ret = (RCLReturnEnum)NativeRcl.rcl_init(
                2, new string[] { "foo", "bar" }, ref init_options, context);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
            Assert.That(NativeRclInterface.rclcs_context_is_valid(context), Is.True);

            RCLInitialize.ShutdownRcl(context);
        }
    }

    [TestFixture]
    public class NodeInitialize
    {
        private IntPtr Context = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
        }

        [TearDown]
        public void TearDown()
        {
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void NodeGetDefaultOptions()
        {
            IntPtr defaultNodeOptions = NativeRclInterface.rclcs_node_create_default_options();
            NativeRclInterface.rclcs_node_dispose_options(defaultNodeOptions);
        }

        internal static IntPtr InitOptions()
        {
            return NativeRclInterface.rclcs_node_create_default_options();
        }

        internal static void ShutdownOptions(IntPtr options)
        {
            NativeRclInterface.rclcs_node_dispose_options(options);
        }

        internal static IntPtr InitNode(IntPtr options, IntPtr context)
        {
            string name = "node_test";
            string nodeNamespace = "/ns";
            IntPtr node = NativeRclInterface.rclcs_get_zero_initialized_node();

            var ret = (RCLReturnEnum)NativeRcl.rcl_node_init(
                node, name, nodeNamespace, context, options);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
            return node;
        }

        internal static void ShutdownNode(IntPtr node)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_node_fini(node);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK));
        }

        [Test]
        public void NodeInitShutdown()
        {
            var options = InitOptions();
            var node = InitNode(options, this.Context);
            ShutdownNode(node);
            ShutdownOptions(options);
        }
    }

    [TestFixture]
    public class NodeTest
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.Options = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.Options, this.Context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.Options);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void NodeGetNamespace()
        {
            string nodeNameFromRcl = Utils.PtrToString(NativeRcl.rcl_node_get_name(this.Node));
            Assert.That("node_test", Is.EqualTo(nodeNameFromRcl));
        }

        [Test]
        public void NodeGetName()
        {
            string nodeNamespaceFromRcl = Utils.PtrToString(NativeRcl.rcl_node_get_namespace(this.Node));
            Assert.That("/ns", Is.EqualTo(nodeNamespaceFromRcl));
        }
    }

    [TestFixture]
    public class PublisherInitialize
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr NodeOptions = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.NodeOptions = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.NodeOptions, this.Context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.NodeOptions);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void PublisherCreateOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr publisherOptions = NativeRclInterface.rclcs_publisher_create_options(qos.handle);
        }

        internal static IntPtr InitOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            return NativeRclInterface.rclcs_publisher_create_options(qos.handle);
        }

        internal static void ShutdownOptions(IntPtr options)
        {
            NativeRclInterface.rclcs_publisher_dispose_options(options);
        }

        internal static IntPtr InitPublisher(IntPtr node, IntPtr options)
        {
            IntPtr publisher = NativeRclInterface.rclcs_get_zero_initialized_publisher();
            MessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            var ret = (RCLReturnEnum)NativeRcl.rcl_publisher_init(
                publisher, node, typeSupportHandle, "publisher_test_topic", options);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            return publisher;
        }

        public static void ShutdownPublisher(IntPtr publisher, IntPtr node)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_publisher_fini(publisher, node);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            NativeRclInterface.rclcs_free_publisher(publisher);
        }

        [Test]
        public void PublisherInit()
        {
            var options = InitOptions();
            var publisher = InitPublisher(this.Node, options);
            ShutdownPublisher(publisher, this.Node);
            ShutdownOptions(options);
        }
    }

    [TestFixture]
    public class PublisherTest
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr NodeOptions = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        private IntPtr PublisherOptions = IntPtr.Zero;

        private IntPtr Publisher = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.NodeOptions = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.NodeOptions, this.Context);
            this.PublisherOptions = PublisherInitialize.InitOptions();
            this.Publisher = PublisherInitialize.InitPublisher(this.Node, this.PublisherOptions);
        }

        [TearDown]
        public void TearDown()
        {
            PublisherInitialize.ShutdownPublisher(this.Publisher, this.Node);
            PublisherInitialize.ShutdownOptions(this.PublisherOptions);
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.NodeOptions);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void PublisherPublish()
        {
            MessageInternals msg = new std_msgs.msg.Bool();
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();

            var ret = (RCLReturnEnum)NativeRcl.rcl_publish(this.Publisher, msg.Handle, allocator.allocate);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
        }
    }

    [TestFixture]
    public class SubscriptionInitialize
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr NodeOptions = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.NodeOptions = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.NodeOptions, this.Context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.NodeOptions);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void SubscriptionCreateOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qos.handle);
            NativeRclInterface.rclcs_subscription_dispose_options(subscriptionOptions);
        }

        internal static IntPtr InitOptions()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            return NativeRclInterface.rclcs_subscription_create_options(qos.handle);      
        }

        internal static void ShutdownOptions(IntPtr options)
        {
            NativeRclInterface.rclcs_subscription_dispose_options(options);
        }

        internal static IntPtr InitSubscription(IntPtr node, IntPtr options)
        {
            IntPtr subscription = NativeRclInterface.rclcs_get_zero_initialized_subscription();      
            MessageInternals msg = new std_msgs.msg.Bool();
            IntPtr typeSupportHandle = msg.TypeSupportHandle;
            var ret = (RCLReturnEnum)NativeRcl.rcl_subscription_init(
                subscription, node, typeSupportHandle, "/subscriber_test_topic", options);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            return subscription;
        }

        internal static void ShutdownSubscription(IntPtr subscription, IntPtr node)
        {
            var ret = (RCLReturnEnum)NativeRcl.rcl_subscription_fini(subscription, node);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
            NativeRclInterface.rclcs_free_subscription(subscription);
        }

        [Test]
        public void SubscriptionInit()
        {
            var options = InitOptions();
            var subscription = InitSubscription(this.Node, options);
            ShutdownSubscription(subscription, this.Node);
            ShutdownOptions(options);
        }
    }

    [TestFixture]
    public class SubscriptionTest
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr NodeOptions = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        private IntPtr SubscriptionOptions = IntPtr.Zero;

        private IntPtr Subscription = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.NodeOptions = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.NodeOptions, this.Context);
            this.SubscriptionOptions = SubscriptionInitialize.InitOptions();
            this.Subscription = SubscriptionInitialize.InitSubscription(this.Node, this.SubscriptionOptions);
        }

        [TearDown]
        public void TearDown()
        {
            SubscriptionInitialize.ShutdownSubscription(this.Subscription, this.Node);
            SubscriptionInitialize.ShutdownOptions(this.SubscriptionOptions);
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.NodeOptions);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void SubscriptionIsValid()
        {
            Assert.That(NativeRclInterface.rclcs_subscription_is_valid(this.Subscription), Is.True);
        }

        [Test]
        public void WaitSetAddSubscription()
        {
            NativeRcl.rcl_reset_error();

            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            IntPtr handle = NativeRclInterface.rclcs_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_init(
                handle,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                this.Context,
                allocator
            ));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_clear(handle));

            Assert.That(NativeRclInterface.rclcs_subscription_is_valid(this.Subscription), Is.True);
            UIntPtr index = (UIntPtr)42;
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_add_subscription(handle, this.Subscription, out index));
            Assert.That(index.ToUInt64(), Is.EqualTo(0));

            long timeout_ns = 10*1000*1000;
            var ret = (RCLReturnEnum)NativeRcl.rcl_wait(handle, timeout_ns);
            Assert.That(ret, Is.EqualTo(RCLReturnEnum.RCL_RET_TIMEOUT));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_fini(handle));
            NativeRclInterface.rclcs_free_wait_set(handle);
        }
    }

    [TestFixture]
    public class WaitSet
    {
        private IntPtr Context = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
        }

        [TearDown]
        public void TearDown()
        {
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void WaitSetInit()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            IntPtr handle = NativeRclInterface.rclcs_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_init(
                handle,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                this.Context,
                allocator
            ));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_fini(handle));
            NativeRclInterface.rclcs_free_wait_set(handle);
        }

        [Test]
        public void WaitSetClear()
        {
            rcl_allocator_t allocator = NativeRcl.rcutils_get_default_allocator();
            IntPtr handle = NativeRclInterface.rclcs_get_zero_initialized_wait_set();
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_init(
                handle,
                (UIntPtr)1,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                (UIntPtr)0,
                this.Context,
                allocator
            ));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_clear(handle));
            TestUtils.AssertRetOk(NativeRcl.rcl_wait_set_fini(handle));
            NativeRclInterface.rclcs_free_wait_set(handle);
        }
    }

    [TestFixture]
    public class QualityOfService
    {
        private IntPtr Context = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private IntPtr Node = IntPtr.Zero;

        [SetUp]
        public void SetUp()
        {
            this.Context = RCLInitialize.InitRcl();
            this.Options = NodeInitialize.InitOptions();
            this.Node = NodeInitialize.InitNode(this.Options, this.Context);
        }

        [TearDown]
        public void TearDown()
        {
            NodeInitialize.ShutdownNode(this.Node);
            NodeInitialize.ShutdownOptions(this.Options);
            RCLInitialize.ShutdownRcl(this.Context);
        }

        [Test]
        public void SetSubscriptionQosProfile()
        {
            QualityOfServiceProfile qos = new QualityOfServiceProfile();
            IntPtr options = NativeRclInterface.rclcs_subscription_create_options(qos.handle);

            var subscription = SubscriptionInitialize.InitSubscription(this.Node, options);

            Assert.That(NativeRclInterface.rclcs_subscription_is_valid(subscription), Is.True);

            SubscriptionInitialize.ShutdownSubscription(subscription, this.Node);
            SubscriptionInitialize.ShutdownOptions(options);
        }
    }

    [TestFixture]
    public class Clock
    {
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
            NativeRcl.rcl_clock_get_now(clockHandle, out queryNow);

            Assert.That(queryNow, Is.Not.EqualTo(0));

            NativeRclInterface.rclcs_ros_clock_dispose(clockHandle);
        }
    }
}
