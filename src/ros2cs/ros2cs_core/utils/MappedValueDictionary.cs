using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ROS2
{
    /// <summary>
    /// Dictionary view which transforms the values of the wrapped dictionary.
    /// </summary>
    internal sealed class MappedValueDictionary<K, T, V> : IReadOnlyDictionary<K, V>
    {
        private readonly IReadOnlyDictionary<K, T> Wrapped;

        private readonly Func<T, V> Mapper;

        public MappedValueDictionary(IReadOnlyDictionary<K, T> wrapped, Func<T, V> mapper)
        {
            this.Wrapped = wrapped;
            this.Mapper = mapper;
        }

        /// <inheritdoc/>
        public V this[K key]
        {
            get { return this.Mapper(this.Wrapped[key]); }
        }

        /// <inheritdoc/>
        public IEnumerable<K> Keys
        {
            get { return this.Wrapped.Keys; }
        }

        /// <inheritdoc/>
        public IEnumerable<V> Values
        {
            get { return this.Wrapped.Values.Select(this.Mapper); }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return this.Wrapped.Count; }
        }

        /// <inheritdoc/>
        public bool ContainsKey(K key)
        {
            return this.Wrapped.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(K key, out V value)
        {
            if (this.Wrapped.TryGetValue(key, out T originalValue))
            {
                value = this.Mapper(originalValue);
                return true;
            }
            else
            {
                value = default(V);
                return false;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.Wrapped
                .Select(pair => new KeyValuePair<K, V>(pair.Key, this.Mapper(pair.Value)))
                .GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
