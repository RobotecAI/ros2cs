using System;
using System.Runtime.InteropServices;

namespace ROS2
{
    internal static class Utils
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

        internal static string GetRclErrorString()
        {
            IntPtr errorStringPtr = NativeRclInterface.rclcs_get_error_string();
            string errorString = PtrToString(errorStringPtr);
            NativeRclInterface.rclcs_dispose_error_string(errorStringPtr);
            return errorString;
        }

        internal static string PopRclErrorString()
        {
            string errorString = GetRclErrorString();
            NativeRcl.rcl_reset_error();
            return errorString;
        }

        internal static string PtrToString(IntPtr p)
        {
          if (p == IntPtr.Zero)
            return null;
          return Marshal.PtrToStringAnsi(p);
        }
    }
}
