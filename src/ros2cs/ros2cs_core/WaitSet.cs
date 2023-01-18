// Copyright 2019-2021 Robotec.ai
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ROS2
{
    /// <summary>
    /// Collection used for waiting on resources to become ready.
    /// </summary>
    internal sealed class WaitSet : IReadOnlyCollection<IWaitable>, IExtendedDisposable
    {
        /// <summary>
        /// The <see cref="ISubscriptionBase"/> instances currently in the wait set.
        /// </summary>
        public ICollection<ISubscriptionBase> Subscriptions { get { return this.CurrentSubscriptions; } }

        /// <summary>
        /// The <see cref="IClientBase"/> instances currently in the wait set.
        /// </summary>
        public ICollection<IClientBase> Clients { get { return this.CurrentClients; } }

        /// <summary>
        /// The <see cref="IServiceBase"/> instances currently in the wait set.
        /// </summary>
        public ICollection<IServiceBase> Services { get { return this.CurrentServices; } }

        /// <summary>
        /// Context associated with this wait set.
        /// </summary>
        public IContext Context { get; private set; }

        /// <inheritdoc />
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRclInterface.rclcs_wait_set_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return this.Subscriptions.Count + this.Clients.Count + this.Services.Count; }
        }

        private IntPtr Handle = IntPtr.Zero;

        /// <summary>
        /// Modification version used to detect if the wait set was modified.
        /// </summary>
        private uint Version = 0;

        // are exposed as collections to prevent users from depending on the changing indexes
        private readonly List<ISubscriptionBase> CurrentSubscriptions = new List<ISubscriptionBase>();

        private readonly List<IClientBase> CurrentClients = new List<IClientBase>();

        private readonly List<IServiceBase> CurrentServices = new List<IServiceBase>();

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="context">Associated context</param>
        internal WaitSet(Context context)
        {
            this.Context = context;
            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_wait_set();
            int ret = NativeRcl.rcl_wait_set_init(
                this.Handle,
                new UIntPtr(Convert.ToUInt32(this.CurrentSubscriptions.Capacity)),
                UIntPtr.Zero,
                UIntPtr.Zero,
                new UIntPtr(Convert.ToUInt32(this.CurrentClients.Capacity)),
                new UIntPtr(Convert.ToUInt32(this.CurrentServices.Capacity)),
                UIntPtr.Zero,
                context.Handle,
                NativeRcl.rcutils_get_default_allocator()
            );
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
            context.OnShutdown += this.Dispose;
        }

        /// <summary>
        /// Assert that the instance has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the instance was disposed</exception>
        private void AssertOk()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("rcl wait set");
            }
        }
        /// <inheritdoc />
        public IEnumerator<IWaitable> GetEnumerator()
        {
            return this.Subscriptions.Concat<IWaitable>(this.Clients).Concat(this.Services).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Wait for something to become ready.
        /// </summary>
        /// <remarks>
        /// This will invalidate previous wait results.
        /// </remarks>
        /// <returns>The resources that became ready</returns>
        /// <exception cref="WaitSetEmptyException">
        /// The wait set can only be waited on if it contains something
        /// </exception>
        public WaitResult Wait()
        {
            if (this.TryWait(TimeSpan.FromTicks(-1), out WaitResult ready))
            {
                return ready;
            }
            // should never happen
            throw new TimeoutException("infinite wait timed out");
        }

        /// <summary>
        /// Resize the wait set to have the same size as the collections holding the resources and clear it.
        /// </summary>
        /// <remarks>
        /// No allocation will be done if the new size of the wait set matches the current size.
        /// </remarks>
        private void PrepareWaitSet()
        {
            Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_resize(
                this.Handle,
                new UIntPtr(Convert.ToUInt32(this.CurrentSubscriptions.Count)),
                UIntPtr.Zero,
                UIntPtr.Zero,
                new UIntPtr(Convert.ToUInt32(this.CurrentClients.Count)),
                new UIntPtr(Convert.ToUInt32(this.CurrentServices.Count)),
                UIntPtr.Zero
            ));
        }

        /// <summary>
        /// Check if the wait set contains something at an index.
        /// </summary>
        /// <param name="getter">Delegate used for accessing the array of that resource</param>
        /// <param name="index">Index to check</param>
        /// <returns>Whether the wait set already contains a resource</returns>
        /// <exception cref="IndexOutOfRangeException">The wait set does not contain the index</exception>
        private bool IsAdded(NativeRclInterface.WaitSetGetType getter, int index)
        {
            if (getter(this.Handle, new UIntPtr(Convert.ToUInt32(index)), out IntPtr ptr))
            {
                return ptr != IntPtr.Zero;
            }
            throw new IndexOutOfRangeException($"wait set has no index {index}");
        }

        /// <summary>
        /// Fill the wait set of a resource.
        /// </summary>
        /// <remarks>
        /// The wrapper will be updated if the wait set adds resources at different indexes.
        /// </remarks>
        /// <typeparam name="T">Type of the resource</typeparam>
        /// <param name="adder">Delegate used for adding to the wait set</param>
        /// <param name="getter">Delegate used for accessing the wait set</param>
        /// <param name="wrappers">Resources to add</param>
        private void FillWaitSet<T>(NativeRcl.WaitSetAddType adder, NativeRclInterface.WaitSetGetType getter, IList<T> wrappers)
            where T : IWaitable
        {
            if (wrappers.Count == 0)
            {
                return;
            }
            int filled = 0;
            int index = 0;
            // add index to wait set until it is filled
            while (true)
            {
                Utils.CheckReturnEnum(adder(this.Handle, wrappers[index].Handle, out UIntPtr destination));
                filled += 1;
                int newIndex = Convert.ToInt32(destination.ToUInt32());
                if (newIndex != index)
                {
                    // different wait set index, update wrappers and repeat with not added resource
                    (wrappers[index], wrappers[newIndex]) = (wrappers[newIndex], wrappers[index]);
                    continue;
                }
                if (filled >= wrappers.Count)
                {
                    // all wrappers filled, skip searching for next index to prevent triggering IndexOutOfRangeException
                    break;
                }
                // some wrappers are not added yet, advance to next index not already in wait set
                // IndexOutOfRangeException indicates that not all wrappers could be added and
                // should not be ignored since it hints at a bug or threading issue
                do
                {
                    index += 1;
                }
                while (this.IsAdded(getter, index));
            }
        }

        /// <summary>
        /// Fill the wait set of all resources.
        /// </summary>
        /// <remarks>
        /// This will clear and resize the wait set first.
        /// </remarks>
        private void FillWaitSet()
        {
            this.PrepareWaitSet();
            this.FillWaitSet(
                NativeRcl.rcl_wait_set_add_subscription,
                NativeRclInterface.rclcs_wait_set_get_subscription,
                this.CurrentSubscriptions
            );
            this.FillWaitSet(
                NativeRcl.rcl_wait_set_add_client,
                NativeRclInterface.rclcs_wait_set_get_client,
                this.CurrentClients
            );
            this.FillWaitSet(
                NativeRcl.rcl_wait_set_add_service,
                NativeRclInterface.rclcs_wait_set_get_service,
                this.CurrentServices
            );
        }

        /// <param name="timeout">Timeout for waiting, infinite if negative</param>
        /// <param name="result">The resources that became ready</param>
        /// <returns>Whether the wait did not timed out</returns>
        /// <inheritdoc cref="Wait"/>
        public bool TryWait(TimeSpan timeout, out WaitResult result)
        {
            this.AssertOk();
            // invalidate last wait result
            this.Version += 1;

            this.FillWaitSet();

            long nanoSeconds = timeout.Ticks * (1_000_000_000 / TimeSpan.TicksPerSecond);
            int ret = NativeRcl.rcl_wait(this.Handle, nanoSeconds);
            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_WAIT_SET_EMPTY)
            {
                throw new WaitSetEmptyException("empty wait set can not be waited on");
            }
            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_TIMEOUT)
            {
                result = default(WaitResult);
                return false;
            }
            Utils.CheckReturnEnum(ret);

            result = new WaitResult(
                new ReadyDictionary<ISubscriptionBase>(
                    this,
                    NativeRclInterface.rclcs_wait_set_get_subscription,
                    NativeRclInterface.rclcs_wait_set_set_subscription,
                    this.CurrentSubscriptions
                ),
                new ReadyDictionary<IClientBase>(
                    this,
                    NativeRclInterface.rclcs_wait_set_get_client,
                    NativeRclInterface.rclcs_wait_set_set_client,
                    this.CurrentClients
                ),
                new ReadyDictionary<IServiceBase>(
                    this,
                    NativeRclInterface.rclcs_wait_set_get_service,
                    NativeRclInterface.rclcs_wait_set_set_service,
                    this.CurrentServices
                )
            );
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposal logic.</summary>
        /// <param name="disposing">If this method is not called in a finalizer</param>
        private void Dispose(bool disposing)
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_wait_set_fini(this.Handle));
            this.FreeHandles();

            if (disposing)
            {
                this.Context.OnShutdown -= this.Dispose;
                this.Subscriptions.Clear();
                this.Clients.Clear();
                this.Services.Clear();
            }
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_wait_set(this.Handle);
            this.Handle = IntPtr.Zero;
        }

        ~WaitSet()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dictionary representing the containts of a wait set.
        /// </summary>
        /// <typeparam name="T">Type of resource being represented</typeparam>
        private sealed class ReadyDictionary<T> : IDictionary<int, T> where T : IWaitable
        {
            /// <inheritdoc />
            public T this[int key]
            {
                get
                {
                    if (this.TryGetValue(key, out T value))
                    {
                        return value;
                    }
                    throw new KeyNotFoundException($"key {key} was not found");
                }
                set { this.Add(key, value); }
            }

            /// <inheritdoc />
            public int Count { get { return this.Keys.Count; } }

            /// <inheritdoc />
            public bool IsReadOnly { get { return true; } }

            /// <inheritdoc />
            public ICollection<int> Keys { get; private set; }

            /// <inheritdoc />
            public ICollection<T> Values { get; private set; }

            private readonly WaitSet WaitSet;

            /// <summary>
            /// Modification version of wait set when created.
            /// </summary>
            private readonly uint CreatedVersion;

            /// <summary>
            /// Delegate used to read the wait set.
            /// </summary>
            private readonly NativeRclInterface.WaitSetGetType Getter;

            /// <summary>
            /// Delegate used to change the wait set.
            /// </summary>
            private readonly NativeRclInterface.WaitSetSetType Setter;

            /// <summary>
            /// Mapping between wait set index and wrapper.
            /// </summary>
            private readonly IList<T> Wrappers;

            internal ReadyDictionary(WaitSet waitSet, NativeRclInterface.WaitSetGetType getter, NativeRclInterface.WaitSetSetType setter, IList<T> wrappers)
            {
                this.WaitSet = waitSet;
                this.CreatedVersion = waitSet.Version;
                this.Getter = getter;
                this.Setter = setter;
                this.Wrappers = wrappers;
                this.Keys = new KeysCollection(this);
                this.Values = new ValuesCollection(this);
            }

            /// <summary>
            /// Check if the wait set is not disposed or was modified.
            /// </summary>
            /// <exception cref="ObjectDisposedException">The wait set has been disposed</exception>
            /// <exception cref="InvalidOperationException">The wait set has been modified</exception>
            private void AssertOk()
            {
                if (this.WaitSet.IsDisposed)
                {
                    throw new ObjectDisposedException("backing rcl wait set");
                }
                if (this.CreatedVersion != this.WaitSet.Version)
                {
                    throw new InvalidOperationException("wait set has been waited on");
                }
            }

            /// <summary>
            /// Try to convert a key to a native index.
            /// </summary>
            /// <param name="key">Key to convert</param>
            /// <param name="nativeIndex">Native index</param>
            /// <returns>Whether the conversion was successful</returns>
            private bool TryConvertKey(int key, out UIntPtr nativeIndex)
            {
                try
                {
                    nativeIndex = new UIntPtr(Convert.ToUInt32(key));
                }
                catch (OverflowException)
                {
                    nativeIndex = UIntPtr.Zero;
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Get the pointer stored at an index.
            /// </summary>
            /// <param name="index">Index of the pointer</param>
            /// <param name="ptr">Pointer stored at index</param>
            /// <returns>Whether the index existed</returns>
            private bool TryGetPtr(UIntPtr index, out IntPtr ptr)
            {
                return this.Getter(this.WaitSet.Handle, index, out ptr);
            }

            /// <summary>
            /// Set the pointer stored at an index.
            /// </summary>
            /// <param name="index">Index of the pointer</param>
            /// <param name="ptr">Pointer to be stored at index</param>
            /// <returns>Whether the index existed</returns>
            private bool TrySetPtr(UIntPtr index, IntPtr ptr)
            {
                return this.Setter(this.WaitSet.Handle, index, ptr);
            }

            /// <inheritdoc />
            public void Add(int key, T value)
            {
                throw new NotSupportedException("adding new elements can only be done on WaitSet");
            }

            /// <inheritdoc />
            public void Add(KeyValuePair<int, T> item)
            {
                this.Add(item.Key, item.Value);
            }

            /// <inheritdoc />
            public bool ContainsKey(int key)
            {
                this.AssertOk();
                return this.TryConvertKey(key, out UIntPtr index) &&
                    this.TryGetPtr(index, out IntPtr ptr) &&
                    ptr != IntPtr.Zero;
            }

            /// <inheritdoc />
            public bool Contains(KeyValuePair<int, T> item)
            {
                return this.TryGetValue(item.Key, out T value) &&
                    EqualityComparer<T>.Default.Equals(item.Value, value);
            }

            /// <inheritdoc />
            public bool Remove(int key)
            {
                this.AssertOk();
                if (this.TryConvertKey(key, out UIntPtr index) &&
                    this.TryGetPtr(index, out IntPtr ptr) &&
                    ptr != IntPtr.Zero)
                {
                    return this.TrySetPtr(index, IntPtr.Zero);
                }
                return false;
            }

            /// <inheritdoc />
            public bool Remove(KeyValuePair<int, T> item)
            {
                return this.Contains(item) && this.Remove(item.Key);
            }

            /// <inheritdoc />
            public void Clear()
            {
                foreach (int key in this.Keys)
                {
                    this.TrySetPtr(new UIntPtr(Convert.ToUInt32(key)), IntPtr.Zero);
                }
            }

            /// <inheritdoc />
            public bool TryGetValue(int key, out T value)
            {
                this.AssertOk();
                if (this.TryConvertKey(key, out UIntPtr index) && this.TryGetPtr(index, out IntPtr ptr) && ptr != IntPtr.Zero)
                {
                    value = this.Wrappers[key];
                    return true;
                }
                value = default(T);
                return false;
            }

            /// <inheritdoc />
            public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
            {
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex is less than 0");
                }
                foreach (var item in this)
                {
                    try
                    {
                        array[arrayIndex] = item;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new ArgumentException("array is too small", e);
                    }
                    arrayIndex += 1;
                }
            }

            /// <inheritdoc />
            public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
            {
                return this.Keys.Select(key => new KeyValuePair<int, T>(key, this.Wrappers[key])).GetEnumerator();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            /// Collection representing the indexes being ready.
            /// </summary>
            private sealed class KeysCollection : ICollection<int>
            {
                /// <inheritdoc />
                public int Count
                {
                    get
                    {
                        // cant use Enumerable.Count() since it
                        // just returns .Count since we are a collection
                        IEnumerator<int> e = this.GetEnumerator();
                        int count = 0;
                        while (e.MoveNext())
                        {
                            checked
                            {
                                count += 1;
                            }
                        }
                        return count;
                    }
                }

                /// <inheritdoc />
                public bool IsReadOnly
                {
                    get { return this.ReadyDictionary.IsReadOnly; }
                }

                private readonly ReadyDictionary<T> ReadyDictionary;

                internal KeysCollection(ReadyDictionary<T> readyDictionary)
                {
                    this.ReadyDictionary = readyDictionary;
                }

                /// <inheritdoc cref="ReadyDictionary{T}.AssertOk"/>
                private void AssertOk()
                {
                    this.ReadyDictionary.AssertOk();
                }

                /// <inheritdoc />
                public void Add(int key)
                {
                    throw new NotSupportedException("adding new elements can only be done on WaitSet");
                }

                /// <inheritdoc />
                public void Clear()
                {
                    this.ReadyDictionary.Clear();
                }

                /// <inheritdoc />
                public bool Contains(int key)
                {
                    return this.ReadyDictionary.ContainsKey(key);
                }

                /// <inheritdoc />
                public void CopyTo(int[] array, int arrayIndex)
                {
                    if (arrayIndex < 0)
                    {
                        throw new ArgumentOutOfRangeException("arrayIndex is less than 0");
                    }
                    foreach (int key in this)
                    {
                        try
                        {
                            array[arrayIndex] = key;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            throw new ArgumentException("array too small", e);
                        }
                        arrayIndex += 1;
                    }
                }

                /// <inheritdoc />
                public bool Remove(int key)
                {
                    return this.ReadyDictionary.Remove(key);
                }

                /// <inheritdoc />
                public IEnumerator<int> GetEnumerator()
                {
                    this.AssertOk();
                    for (int key = 0; this.ReadyDictionary.TryConvertKey(key, out UIntPtr index) && this.ReadyDictionary.TryGetPtr(index, out IntPtr ptr); key += 1)
                    {
                        if (ptr != IntPtr.Zero)
                        {
                            yield return key;
                            // can be invalidated before being resumed
                            this.AssertOk();
                        }
                    }
                }

                /// <inheritdoc />
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }

            /// <summary>
            /// Collection representing the wrappers which are ready.
            /// </summary>
            private sealed class ValuesCollection : ICollection<T>
            {
                /// <inheritdoc />
                public int Count
                {
                    get { return this.ReadyDictionary.Count; }
                }

                /// <inheritdoc />
                public bool IsReadOnly
                {
                    get { return this.ReadyDictionary.IsReadOnly; }
                }

                private readonly ReadyDictionary<T> ReadyDictionary;

                internal ValuesCollection(ReadyDictionary<T> readDictionary)
                {
                    this.ReadyDictionary = readDictionary;
                }

                /// <inheritdoc />
                public void Add(T value)
                {
                    throw new NotSupportedException("adding new elements can only be done on WaitSet");
                }

                /// <inheritdoc />
                public void Clear()
                {
                    this.ReadyDictionary.Clear();
                }

                /// <inheritdoc />
                public bool Contains(T value)
                {
                    return this.Any(v => EqualityComparer<T>.Default.Equals(v, value));
                }

                /// <inheritdoc />
                public void CopyTo(T[] array, int arrayIndex)
                {
                    if (arrayIndex < 0)
                    {
                        throw new ArgumentOutOfRangeException("arrayIndex is less than 0");
                    }
                    foreach (T value in this)
                    {
                        try
                        {
                            array[arrayIndex] = value;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            throw new ArgumentException("array too small", e);
                        }
                        arrayIndex += 1;
                    }
                }

                /// <inheritdoc />
                public bool Remove(T value)
                {
                    foreach (var item in this.ReadyDictionary)
                    {
                        if (EqualityComparer<T>.Default.Equals(item.Value, value))
                        {
                            return this.ReadyDictionary.Remove(item.Key);
                        }
                    }
                    return false;
                }

                /// <inheritdoc />
                public IEnumerator<T> GetEnumerator()
                {
                    return this.ReadyDictionary.Select(item => item.Value).GetEnumerator();
                }

                /// <inheritdoc />
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }
        }
    }

    /// <summary>
    /// Result of waiting on a wait set.
    /// </summary>
    /// <remarks>
    /// The dictionaries are invalidated when waiting on the wait set again.
    /// </remarks>
    internal sealed class WaitResult : IEnumerable<IWaitable>
    {
        /// <summary>
        /// Mapping from index to subscriptions which are ready.
        /// </summary>
        public IDictionary<int, ISubscriptionBase> ReadySubscriptions { get; private set; }

        /// <summary>
        /// Mapping from index to clients which are ready.
        /// </summary>
        public IDictionary<int, IClientBase> ReadyClients { get; private set; }

        /// <summary>
        /// Mapping from index to services which are ready.
        /// </summary>
        public IDictionary<int, IServiceBase> ReadyServices { get; private set; }

        internal WaitResult(IDictionary<int, ISubscriptionBase> subscriptions, IDictionary<int, IClientBase> clients, IDictionary<int, IServiceBase> services)
        {
            this.ReadySubscriptions = subscriptions;
            this.ReadyClients = clients;
            this.ReadyServices = services;
        }

        /// <inheritdoc />
        public IEnumerator<IWaitable> GetEnumerator()
        {
            return this.ReadySubscriptions.Values
                .Concat<IWaitable>(this.ReadyClients.Values)
                .Concat(this.ReadyServices.Values)
                .GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Deconstruct the result into the resources which are ready.
        /// </summary>
        public void Deconstruct(out IDictionary<int, ISubscriptionBase> subscriptions, out IDictionary<int, IClientBase> clients, out IDictionary<int, IServiceBase> services)
        {
            subscriptions = this.ReadySubscriptions;
            clients = this.ReadyClients;
            services = this.ReadyServices;
        }
    }
}
