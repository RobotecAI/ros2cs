using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ROS2
{
    /// <summary>
    /// Collection view which locks the the wrapped collection using an object as lock.
    /// </summary>
    internal sealed class LockedCollection<T> : IReadOnlyCollection<T>
    {
        private readonly IReadOnlyCollection<T> Wrapped;

        private readonly object Lock;

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                lock (this.Lock)
                {
                    return this.Wrapped.Count;
                }
            }
        }

        public LockedCollection(IReadOnlyCollection<T> wrapped) : this(wrapped, wrapped)
        {}

        public LockedCollection(IReadOnlyCollection<T> wrapped, object _lock)
        {
            this.Wrapped = wrapped;
            this.Lock = _lock;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            lock (this.Lock)
            {
                return this.Wrapped.ToArray().AsEnumerable().GetEnumerator();
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}