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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ROS2.Executors
{
    /// <summary>
    /// Executor which has to be spun manually.
    /// </summary>
    /// <remarks>
    /// Spinning is impossible if a rescan is scheduled
    /// to allow waiting to stop when the executor is not spinning.
    /// </remarks>
    public sealed class ManualExecutor : IExecutor
    {
        /// <summary>
        /// Context associated with this executor.
        /// </summary>
        public IContext Context
        {
            get { return this.WaitSet.Context; }
        }

        /// <summary>
        /// Whether the executor is currently spinning.
        /// </summary>
        public bool IsSpinning
        {
            get { return !this.IsIdle.IsSet; }
        }

        /// <summary>
        /// Whether a rescan is scheduled.
        /// </summary>
        public bool RescanScheduled
        {
            get { return this._RescanScheduled; }
            private set { this._RescanScheduled = value; }
        }

        // volatile since it may be changed by multiple threads
        private volatile bool _RescanScheduled = false;

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get { return this.WaitSet.IsDisposed || this.InterruptCondition.IsDisposed; }
        }

        /// <remarks>
        /// This property is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                lock (this.Nodes)
                {
                    return this.Nodes.Count;
                }
            }
        }

        /// <remarks>
        /// This property is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Wait set used while spinning.
        /// </summary>
        private readonly WaitSet WaitSet;

        /// <summary>
        /// Guard condition used for interrupting waits.
        /// </summary>
        private readonly GuardCondition InterruptCondition;

        /// <summary>
        /// Nodes in the executor.
        /// </summary>
        private readonly HashSet<INode> Nodes = new HashSet<INode>();

        /// <summary>
        /// Event signaling whether the executor is not spinning.
        /// </summary>
        private readonly ManualResetEventSlim IsIdle = new ManualResetEventSlim(true);

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="context"> Context to associate with. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="context"/> is disposed. </exception>
        public ManualExecutor(Context context) : this(
            context.CreateWaitSet(),
            context.CreateGuardCondition(() => { })
        )
        { }

        /// <inheritdoc/>
        internal ManualExecutor(WaitSet waitSet, GuardCondition interruptCondition)
        {
            this.WaitSet = waitSet;
            this.InterruptCondition = interruptCondition;
            this.WaitSet.GuardConditions.Add(this.InterruptCondition);
        }

        /// <remarks>
        /// This method is thread safe when setting
        /// <see cref="INode.Executor"/> is thread safe
        /// and not changed concurrently.
        /// </remarks>
        /// <exception cref="InvalidOperationException"> If the node already has an executor. </exception>
        /// <inheritdoc/>
        public void Add(INode node)
        {
            if (!(node.Executor is null))
            {
                throw new InvalidOperationException("node already has an executor");
            }
            // make sure the node knows its
            // new executor before it can be added to the wait set
            node.Executor = this;
            lock (this.Nodes)
            {
                this.Nodes.Add(node);
            }
            this.ScheduleRescan();
        }

        /// <remarks>
        /// This method is thread safe when setting
        /// <see cref="INode.Executor"/> is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the executor was disposed. </exception>
        /// <inheritdoc/>
        public bool Remove(INode node)
        {
            bool removed;
            lock (this.Nodes)
            {
                removed = this.Nodes.Remove(node);
            }
            if (removed)
            {
                try
                {
                    Debug.Assert(
                        Object.ReferenceEquals(node.Executor, this),
                        "node has different executor"
                    );
                    this.ScheduleRescan();
                    this.Wait();
                }
                finally
                {
                    // clear executor after it
                    // is safe to do so
                    node.Executor = null;
                }
            }
            return removed;
        }

        /// <remarks>
        /// This method is thread safe when setting
        /// <see cref="INode.Executor"/> is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the executor was disposed. </exception>
        /// <inheritdoc/>
        public void Clear()
        {
            // use thread safe enumerator
            foreach (INode node in this)
            {
                this.Remove(node);
            }
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool Contains(INode node)
        {
            lock (this.Nodes)
            {
                return this.Nodes.Contains(node);
            }
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public void CopyTo(INode[] array, int index)
        {
            if (array is null)
            {
                throw new ArgumentException("array is null");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index is less than 0");
            }
            lock (this.Nodes)
            {
                foreach (INode item in this.Nodes)
                {
                    try
                    {
                        array[index] = item;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new ArgumentException("array is too small", e);
                    }
                    index += 1;
                }
            }
        }

        /// <remarks>
        /// The enumerator is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public IEnumerator<INode> GetEnumerator()
        {
            lock (this.Nodes)
            {
                return this.Nodes.ToArray().AsEnumerable().GetEnumerator();
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public void ScheduleRescan()
        {
            this.RescanScheduled = true;
        }

        /// <remarks>
        /// This method is an alias for <see cref="ScheduleRescan"/>.
        /// </remarks>
        /// <inheritdoc/>
        public bool TryScheduleRescan(INode node)
        {
            this.ScheduleRescan();
            return true;
        }

        /// <remarks>
        /// This method is thread safe and uses <see cref="TryWait"/>.
        /// </remarks>
        /// <inheritdoc/>
        public void Wait()
        {
            bool success = this.TryWait(TimeSpan.FromMilliseconds(-1));
            Debug.Assert(success, "infinite wait timed out");
        }

        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the executor was disposed. </exception>
        /// <inheritdoc/>
        public bool TryWait(TimeSpan timeout)
        {
            if (this.RescanScheduled && this.IsSpinning)
            {
                try
                {
                    this.Interrupt();
                }
                catch (ObjectDisposedException)
                {
                    // if the context is shut down then the
                    // guard condition might be disposed but
                    // nodes still have to be removed
                }
                return this.IsIdle.Wait(timeout);
            }
            return true;
        }

        /// <summary>
        /// Interrupt the next or current <see cref="TrySpin"/>.
        /// </summary>
        /// <remarks>
        /// This method only causes the wait to be skipped, work which is ready will be executed.
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the executor or context was disposed. </exception>
        public void Interrupt()
        {
            this.InterruptCondition.Trigger();
        }

        /// <summary>
        /// Try to process work if no rescan is scheduled.
        /// </summary>
        /// <remarks>
        /// This method is thread safe if it itself or <see cref="Rescan"/> is not executed concurrently.
        /// </remarks>
        /// <param name="timeout"> Maximum time to wait for work to become available. </param>
        /// <exception cref="ObjectDisposedException"> If the executor or context was disposed. </exception>
        /// <returns> Whether work could be processed since no rescan was scheduled. </returns>
        public bool TrySpin(TimeSpan timeout)
        {
            this.IsIdle.Reset();
            try
            {
                // check after resetting IsIdle to
                // prevent race condition
                if (this.RescanScheduled)
                {
                    return false;
                }
                if (this.WaitSet.TryWait(timeout, out var result))
                {
                    foreach (IWaitable waitable in result)
                    {
                        waitable.TryProcess();
                    }
                }
            }
            finally
            {
                this.IsIdle.Set();
            }
            return true;
        }

        /// <summary>
        /// Rescan the nodes of this executor for
        /// new objects to wait for and clear scheduled rescans.
        /// </summary>
        /// <remarks>
        /// This method is thread safe if it itself or <see cref="TrySpin"/> is not executed concurrently
        /// and enumerating the primitives of the nodes is thread safe.
        /// </remarks>
        public void Rescan()
        {
            // clear the wait set first to
            // assert that the removed objects
            // can be disposed even on error
            this.WaitSet.Subscriptions.Clear();
            this.WaitSet.Services.Clear();
            this.WaitSet.Clients.Clear();
            // clear the flag to prevent clearing rescans
            // scheduled just after we finished rescaning
            this.RescanScheduled = false;
            try
            {
                // use the thread safe GetEnumerator wrapper
                foreach (INode node in this)
                {
                    foreach (ISubscriptionBase subscription in node.Subscriptions)
                    {
                        this.WaitSet.Subscriptions.Add(subscription);
                    }
                    foreach (IServiceBase service in node.Services)
                    {
                        this.WaitSet.Services.Add(service);
                    }
                    foreach (IClientBase client in node.Clients)
                    {
                        this.WaitSet.Clients.Add(client);
                    }
                }
            }
            catch (Exception)
            {
                // schedule rescan since the wait set may not be filled completely
                this.ScheduleRescan();
                throw;
            }
        }

        /// <summary>
        /// Utility which spins while a condition is true
        /// and handles automatic rescanning.
        /// </summary>
        /// <remarks>
        /// The condition check is performed before each spin.
        /// </remarks>
        /// <param name="condition"> Condition which has to be true to continue spinning. </param>
        public void SpinWhile(Func<bool> condition)
        {
            this.SpinWhile(condition, TimeSpan.FromSeconds(0.1));
        }

        /// <inheritdoc cref="SpinWhile"/>
        /// <param name="timeout"> Maximum time to wait for work to become available. </param>
        public void SpinWhile(Func<bool> condition, TimeSpan timeout)
        {
            while (condition())
            {
                if (!this.TrySpin(timeout))
                {
                    this.Rescan();
                }
            }
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the executor is in use.
        /// Furthermore, it does not dispose the nodes of this executor.
        /// Remember that <see cref="Clear"/> is called when disposing
        /// with nodes in the executor.
        /// </remarks>
        /// <inheritdoc/>
        public void Dispose()
        {
            // remove nodes one by one to
            // prevent node.Executor from being out
            // of sync if an exception occurs
            foreach (INode node in this.Nodes.ToArray())
            {
                this.Nodes.Remove(node);
                // waiting not required since the executor
                // should not be running
                node.Executor = null;
            }
            this.WaitSet.Dispose();
            this.InterruptCondition.Dispose();
            this.IsIdle.Dispose();
        }
    }
}