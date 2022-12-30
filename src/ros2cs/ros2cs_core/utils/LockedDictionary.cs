using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ROS2
{
    /// <summary>
    /// Dictionary view which locks the the wrapped dictionary using an object as lock.
    /// </summary>
    internal sealed class LockedDictionary<K, V> : IReadOnlyDictionary<K, V>
    {
        private readonly IReadOnlyDictionary<K, V> Wrapped;

        private readonly object Lock;

        public LockedDictionary(IReadOnlyDictionary<K, V> wrapped) : this(wrapped, wrapped)
        { }

        public LockedDictionary(IReadOnlyDictionary<K, V> wrapped, object _lock)
        {
            this.Wrapped = wrapped;
            this.Lock = _lock;
        }

        /// <inheritdoc/>
        public V this[K key]
        {
            get
            {
                lock (this.Lock)
                {
                    return this.Wrapped[key];
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<K> Keys
        {
            get
            {
                lock (this.Lock)
                {
                    return this.Wrapped.Keys.ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<V> Values
        {
            get
            {
                lock (this.Lock)
                {
                    return this.Wrapped.Values.ToArray();
                }
            }
        }

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

        /// <inheritdoc/>
        public bool ContainsKey(K key)
        {
            lock (this.Lock)
            {
                return this.Wrapped.ContainsKey(key);
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue(K key, out V value)
        {
            lock (this.Lock)
            {
                return this.Wrapped.TryGetValue(key, out value);
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
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
