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
    /// All methods and properties are NOT thread safe.
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
        /// The <see cref="GuardCondition"/> instances currently in the wait set.
        /// </summary>
        public ICollection<GuardCondition> GuardConditions { get { return this.CurrentGuardConditions; } }

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
            get
            {
                return this.Subscriptions.Count
                    + this.Clients.Count
                    + this.Services.Count
                    + this.GuardConditions.Count;
            }
        }

        /// <summary>
        /// Handle to the rcl wait set.
        /// </summary>
        private IntPtr Handle = IntPtr.Zero;

        /// <summary>
        /// Modification version used to detect if the wait set was modified.
        /// </summary>
        private uint Version = 0;

        // are exposed as collections to prevent users from depending on the changing indexes
        private readonly List<ISubscriptionBase> CurrentSubscriptions = new List<ISubscriptionBase>();

        private readonly List<IClientBase> CurrentClients = new List<IClientBase>();

        private readonly List<IServiceBase> CurrentServices = new List<IServiceBase>();

        private readonly List<GuardCondition> CurrentGuardConditions = new List<GuardCondition>();

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
                new UIntPtr(Convert.ToUInt32(this.CurrentGuardConditions.Capacity)),
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

        /// <inheritdoc />
        public IEnumerator<IWaitable> GetEnumerator()
        {
            return this.Subscriptions
                .Concat<IWaitable>(this.Clients)
                .Concat(this.Services)
                .Concat(this.GuardConditions)
                .GetEnumerator();
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
            int ret = NativeRcl.rcl_wait_set_resize(
                this.Handle,
                new UIntPtr(Convert.ToUInt32(this.CurrentSubscriptions.Count)),
                new UIntPtr(Convert.ToUInt32(this.CurrentGuardConditions.Count)),
                UIntPtr.Zero,
                new UIntPtr(Convert.ToUInt32(this.CurrentClients.Count)),
                new UIntPtr(Convert.ToUInt32(this.CurrentServices.Count)),
                UIntPtr.Zero
            );
            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_INVALID_ARGUMENT)
            {
                throw new ObjectDisposedException("RCL wait set");
            }
            Utils.CheckReturnEnum(ret);
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
            this.FillWaitSet(
                NativeRcl.rcl_wait_set_add_guard_condition,
                NativeRclInterface.rclcs_wait_set_get_guard_condition,
                this.CurrentGuardConditions
            );
        }

        /// <param name="timeout">Timeout for waiting, infinite if negative</param>
        /// <param name="result">The resources that became ready</param>
        /// <returns>Whether the wait did not timed out</returns>
        /// <inheritdoc cref="Wait"/>
        public bool TryWait(TimeSpan timeout, out WaitResult result)
        {
            // invalidate last wait result
            this.Version += 1;

            this.FillWaitSet();

            long nanoSeconds = timeout.Ticks * (1_000_000_000 / TimeSpan.TicksPerSecond);
            int ret = NativeRcl.rcl_wait(this.Handle, nanoSeconds);
            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_TIMEOUT)
            {
                result = default(WaitResult);
                return false;
            }
            Utils.CheckReturnEnum(ret);

            result = new WaitResult(this);
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
        /// Result of waiting on a wait set.
        /// </summary>
        /// <remarks>
        /// The enumerables are invalidated when waiting on the wait set again,
        /// which is only done for debugging purposes and not done when the
        /// collections containing the primitives change.
        /// </remarks>
        public struct WaitResult : IEnumerable<IWaitable>
        {
            /// <summary>
            /// Subscriptions which are ready.
            /// </summary>
            public IEnumerable<ISubscriptionBase> ReadySubscriptions
            {
                get => this.CurrentPrimitives(
                    NativeRclInterface.rclcs_wait_set_get_subscription,
                    this.WaitSet.CurrentSubscriptions
                );
            }

            /// <summary>
            /// Clients which are ready.
            /// </summary>
            public IEnumerable<IClientBase> ReadyClients
            {
                get => this.CurrentPrimitives(
                    NativeRclInterface.rclcs_wait_set_get_client,
                    this.WaitSet.CurrentClients
                );
            }

            /// <summary>
            /// Services which are ready.
            /// </summary>
            public IEnumerable<IServiceBase> ReadyServices
            {
                get => this.CurrentPrimitives(
                    NativeRclInterface.rclcs_wait_set_get_service,
                    this.WaitSet.CurrentServices
                );
            }

            /// <summary>
            /// Guard conditions which are ready.
            /// </summary>
            public IEnumerable<GuardCondition> ReadyGuardConditions
            {
                get => this.CurrentPrimitives(
                    NativeRclInterface.rclcs_wait_set_get_guard_condition,
                    this.WaitSet.CurrentGuardConditions
                );
            }

            /// <summary>
            /// Wait set associated with this result.
            /// </summary>
            private readonly WaitSet WaitSet;

            /// <summary>
            /// Version when this result was created.
            /// </summary>
            private readonly uint CreatedVersion;

            internal WaitResult(WaitSet waitSet)
            {
                this.WaitSet = waitSet;
                this.CreatedVersion = waitSet.Version;
            }

            /// <summary>
            /// Assert that the wait set is valid and has not been waited on.
            /// </summary>
            /// <exception cref="ObjectDisposedException"> If the wait set was disposed. </exception>
            private void AssertOk()
            {
                if (this.WaitSet.Version != this.CreatedVersion || this.WaitSet.IsDisposed)
                {
                    throw new ObjectDisposedException("rcl wait set");
                }
            }

            /// <summary>
            /// Primitives currently in the wait set.
            /// </summary>
            /// <remarks>
            /// After waiting the only primitives left in
            /// the wait set are ready.
            /// </remarks>
            /// <typeparam name="T"> Primitive type </typeparam>
            /// <param name="getter"> Function to access the wait set array. </param>
            /// <param name="primitives"> List holding the primitives. </param>
            /// <returns>Enumerable of the primitives. </returns>
            private IEnumerable<T> CurrentPrimitives<T>(NativeRclInterface.WaitSetGetType getter, IList<T> primitives) where T : IWaitable
            {
                this.AssertOk();
                for (UIntPtr index = UIntPtr.Zero; getter(this.WaitSet.Handle, index, out IntPtr ptr); index += 1)
                {
                    if (ptr != IntPtr.Zero)
                    {
                        yield return primitives[Convert.ToInt32(index.ToUInt32())];
                        this.AssertOk();
                    }
                }
            }

            /// <inheritdoc />
            public IEnumerator<IWaitable> GetEnumerator()
            {
                return this.ReadySubscriptions
                    .Concat<IWaitable>(this.ReadyClients)
                    .Concat(this.ReadyServices)
                    .Concat(this.ReadyGuardConditions)
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
            public void Deconstruct(
                out IEnumerable<ISubscriptionBase> subscriptions,
                out IEnumerable<IClientBase> clients,
                out IEnumerable<IServiceBase> services,
                out IEnumerable<GuardCondition> guard_conditions)
            {
                subscriptions = this.ReadySubscriptions;
                clients = this.ReadyClients;
                services = this.ReadyServices;
                guard_conditions = this.ReadyGuardConditions;
            }
        }
    }
}
