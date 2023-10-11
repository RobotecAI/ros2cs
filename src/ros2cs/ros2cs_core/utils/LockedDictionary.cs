// Copyright 2023 ADVITEC Informatik GmbH - www.advitec.de
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
