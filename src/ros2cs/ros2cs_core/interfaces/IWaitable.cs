using System;

namespace ROS2
{
    /// <summary>
    /// Object which can process becoming ready.
    /// </summary>
    public interface IWaitable
    {
        /// <summary>
        /// The handle used for adding to a wait set.
        /// </summary>
        IntPtr Handle { get; }

        /// <summary>
        /// Try to process if this instance is ready.
        /// </summary>
        /// <returns> If the instance was ready. </returns>
        bool TryProcess();
    }
}
