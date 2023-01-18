using System;
using System.Threading.Tasks;

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
        /// <remarks>Both variants of this method are equivalent.</remarks>
        /// <returns>If the instance was ready.</returns>
        bool TryProcess();

        /// <summary>
        /// Try to process preferably asynchronously if this instance is ready.
        /// </summary>
        /// <returns><see cref="Task"/> indicating if the instance was ready.</returns>
        /// <inheritdoc cref="TryProcess"/>
        Task<bool> TryProcessAsync();
    }
}
