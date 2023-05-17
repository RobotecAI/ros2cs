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