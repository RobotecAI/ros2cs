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

        public bool IsSpinning
        {
            get { return !this.IsIdle.IsSet; }
        }

        public bool RescanScheduled
        {
            get { return this._RescanScheduled; }
        }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get { return this.WaitSet.IsDisposed || this.InterruptCondition.IsDisposed; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return this.Nodes.Count; }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        private readonly WaitSet WaitSet;

        private readonly GuardCondition InterruptCondition;

        private readonly HashSet<INode> Nodes = new HashSet<INode>();

        private readonly ManualResetEventSlim IsIdle = new ManualResetEventSlim(true);

        private volatile bool _RescanScheduled = false;

        public ManualExecutor(Context context) : this(
            new WaitSet(context),
            new GuardCondition(context, () => { })
        )
        { }

        internal ManualExecutor(WaitSet waitSet, GuardCondition interruptCondition)
        {
            this.WaitSet = waitSet;
            this.InterruptCondition = interruptCondition;
            this.WaitSet.GuardConditions.Add(this.InterruptCondition);
        }

        /// <inheritdoc/>
        public void Add(INode node)
        {
            if (!(node.Executor is null))
            {
                throw new InvalidOperationException("node already has an executor");
            }
            this.Nodes.Add(node);
            node.Executor = this;
            this.ScheduleRescan();
        }

        /// <inheritdoc/>
        public bool Remove(INode node)
        {
            if (this.Nodes.Remove(node))
            {
                Debug.Assert(
                    Object.ReferenceEquals(node.Executor, this),
                    "node has different executor"
                );
                node.Executor = null;
                this.ScheduleRescan();
                this.Wait();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (this.Nodes.Count == 0)
            {
                return;
            }
            foreach (INode node in this.Nodes.ToArray())
            {
                this.Nodes.Remove(node);
                node.Executor = null;
            }
            this.ScheduleRescan();
            this.Wait();
        }

        /// <inheritdoc/>
        public bool Contains(INode node)
        {
            return this.Nodes.Contains(node);
        }

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
            foreach (var item in this)
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

        /// <inheritdoc/>
        public IEnumerator<INode> GetEnumerator()
        {
            return this.Nodes.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public void ScheduleRescan()
        {
            this._RescanScheduled = true;
        }

        /// <inheritdoc/>
        public bool TryScheduleRescan(INode node)
        {
            this.ScheduleRescan();
            return true;
        }

        /// <inheritdoc/>
        public void Wait()
        {
            bool success = this.TryWait(TimeSpan.FromMilliseconds(-1));
            Debug.Assert(success, "infinite wait timed out");
        }

        /// <inheritdoc/>
        public bool TryWait(TimeSpan timeout)
        {
            if (this._RescanScheduled)
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
        /// Interrupt <see cref="TrySpin"/> if waiting for
        /// work to become available.
        /// </summary>
        /// <remarks>
        /// If <see cref="TrySpin"/> is not currently waiting then
        /// this method does not have any effect.
        /// Furthermore no work is skipped.
        /// </remarks>
        public void Interrupt()
        {
            this.InterruptCondition.Trigger();
        }

        /// <summary>
        /// Try to process work if no rescan is scheduled.
        /// </summary>
        /// <param name="timeout"> Maximum time to wait for work to become available. </param>
        /// <returns> Whether work could be processed since no rescan was scheduled. </returns>
        public bool TrySpin(TimeSpan timeout)
        {
            this.IsIdle.Reset();
            try
            {
                if (this._RescanScheduled)
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
        /// new objects to wait for.
        /// </summary>
        /// <remarks> This clears any scheduled rescans. </remarks>
        public void Rescan()
        {
            this.WaitSet.Subscriptions.Clear();
            this.WaitSet.Services.Clear();
            this.WaitSet.Clients.Clear();
            // clear it first to prevent clearing rescans
            // scheduled just after we finished rescaning
            this._RescanScheduled = false;
            foreach (INode node in this.Nodes)
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

        /// <summary>
        /// Utility which handles automatic rescaning.
        /// </summary>
        /// <param name="timeout"> Maximum time to wait for work to become available. </param>
        /// <returns>
        /// <see cref="IEnumerator{bool}"/> trying to spin in each iteration
        /// and yielding if a rescan had to be performed.
        /// </returns>
        public IEnumerator<bool> Spin(TimeSpan timeout)
        {
            while (true)
            {
                if (this.TrySpin(timeout))
                {
                    yield return false;
                }
                else
                {
                    this.Rescan();
                    yield return true;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Clear();
            this.WaitSet.Dispose();
            this.InterruptCondition.Dispose();
            this.IsIdle.Dispose();
        }
    }
}