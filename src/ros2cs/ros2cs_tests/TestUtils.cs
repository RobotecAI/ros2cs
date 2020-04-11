using NUnit.Framework;
using System;

namespace ROS2.Test
{
    static class TestUtils
    {
        public static void AssertRetOk(int ret)
        {
            Assert.That((RCLReturnEnum)ret, Is.EqualTo(RCLReturnEnum.RCL_RET_OK), Utils.PopRclErrorString());
        }
    }
}
