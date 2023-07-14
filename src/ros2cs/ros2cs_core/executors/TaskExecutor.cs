using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ROS2.Executors
{
    /// <summary>
    /// Executor which wraps a <see cref="ManualExecutor"/> and automatically
    /// executes the task created by <see cref="ManualExecutor.CreateSpinTask"/>.
    /// </summary>
    /// <remarks>
    /// The spin task is automatically stopped when <see cref="Dispose"/>
    /// is called or the context is shut down.
    /// </remarks>
    public sealed class TaskExecutor : IExecutor
    {
        /// <summary>
        /// Task managed by this executor.
        /// </summary>
        public Task Task { get; private set; }

        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        private readonly ManualExecutor Executor;

        private readonly Context Context;

        /// <param name="context"> Context associated with this executor. </param>
        /// <param name="timeout"> Maximum time to wait for work to become available. </param>
        public TaskExecutor(Context context, TimeSpan timeout)
        {
            this.Context = context;
            this.Executor = new ManualExecutor(context);
            this.Task = this.Executor.CreateSpinTask(timeout, this.CancellationSource.Token);
            try
            {
                context.OnShutdown += this.StopSpinTask;
                this.Task.Start();
            }
            catch (SystemException)
            {
                try
                {
                    context.OnShutdown -= this.StopSpinTask;
                }
                finally
                {
                    this.Executor.Dispose();
                }
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get => this.Executor.IsDisposed;
        }

        /// <inheritdoc/>
        public int Count
        {
            get => this.Executor.Count;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get => this.Executor.IsReadOnly;
        }

        /// <inheritdoc/>
        public void Add(INode node)
        {
            this.Executor.Add(node);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.Executor.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(INode node)
        {
            return this.Executor.Contains(node);
        }

        /// <inheritdoc/>
        public void CopyTo(INode[] array, int arrayIndex)
        {
            this.Executor.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(INode node)
        {
            return this.Executor.Remove(node);
        }

        /// <inheritdoc/>
        public IEnumerator<INode> GetEnumerator()
        {
            return this.Executor.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public void ScheduleRescan()
        {
            this.Executor.ScheduleRescan();
        }

        /// <inheritdoc />
        public bool TryScheduleRescan(INode node)
        {
            return this.Executor.TryScheduleRescan(node);
        }

        /// <inheritdoc />
        public void Wait()
        {
            this.Executor.Wait();
        }

        /// <inheritdoc />
        public bool TryWait(TimeSpan timeout)
        {
            return this.Executor.TryWait(timeout);
        }

        /// <summary>
        /// Stop the spin task and return after it has stopped.
        /// </summary>
        /// <remarks>
        /// This function returns immediately if the spin task
        /// has already been stopped.
        /// </remarks>
        private void StopSpinTask()
        {
            try
            {
                this.CancellationSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // task has been canceled before
            }
            try
            {
                this.Task.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(inner => inner is TaskCanceledException);
            }
            catch (ObjectDisposedException)
            {
                // task has already stopped
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// The wrapper handles stopping the spin task.
        /// </remarks>
        public void Dispose()
        {
            try
            {
                this.StopSpinTask();
            }
            catch (AggregateException)
            {
                // prevent faulted task from preventing disposal
            }
            this.Context.OnShutdown -= this.StopSpinTask;
            this.Task.Dispose();
            this.Executor.Dispose();
            this.CancellationSource.Dispose();
        }
    }
}