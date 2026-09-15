// SPDX-License-Identifier: Apache-2.0
using System;
using NUnit.Framework;

namespace ROS2.Test
{
    public class WaitSetLifetimeTest
    {
        [Test]
        public void MultipleSubscriptionsClientsAndServicesSurviveCollections()
        {
            Ros2cs.Init();
            try
            {
                var node = Ros2cs.CreateNode("waitset_lifetime_test");
                for (int i = 0; i < 8; i++)
                {
                    node.CreateSubscription<std_msgs.msg.Int32>("waitset_topic", _ => { });
                    node.CreateService<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response>(
                        "waitset_service_" + i, request => new example_interfaces.srv.AddTwoInts_Response { Sum = request.A + request.B });
                    node.CreateClient<example_interfaces.srv.AddTwoInts_Request, example_interfaces.srv.AddTwoInts_Response>("waitset_service_" + i);
                }
                for (int i = 0; i < 20; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Assert.DoesNotThrow(() => Ros2cs.SpinOnce(node, 0.01));
                }
            }
            finally { Ros2cs.Shutdown(); }
        }
    }
}
