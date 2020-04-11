using System;

namespace ROS2
{
    public static class Utils
    {
        internal static void CheckReturnEnum(int ret)
        {
            string errorMessage = Utils.PopRclErrorString();

            switch ((RCLReturnEnum)ret)
            {
                case RCLReturnEnum.RCL_RET_OK:
                    break;
                case RCLReturnEnum.RCL_RET_NODE_INVALID_NAME:
                    throw new InvalidNodeNameException(errorMessage);
                case RCLReturnEnum.RCL_RET_NODE_INVALID_NAMESPACE:
                    throw new InvalidNamespaceException(errorMessage);
                default:
                    throw new RuntimeError(errorMessage);
            }
        }

        public static string GetRclErrorString()
        {
            IntPtr errorStringPtr = NativeMethods.rclcs_get_error_string();
            string errorString = MarshallingHelpers.PtrToString(errorStringPtr);
            NativeMethods.rclcs_dispose_error_string(errorStringPtr);
            return errorString;
        }

        public static string PopRclErrorString()
        {
            string errorString = GetRclErrorString();
            NativeMethods.rcl_reset_error();
            return errorString;
        }

        public static ulong TimeoutSecToNsec(double timeoutSec)
        {
            const double S_TO_NS = 1000 * 1000 * 1000;
            if(timeoutSec < 0)
            {
                throw new RuntimeError("Negative timeouts are not allowed, timeout was " + timeoutSec + " seconds.");
            }
            return (ulong)(timeoutSec * S_TO_NS);
        }
    }
}
