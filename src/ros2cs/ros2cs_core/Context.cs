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
using System.Collections.Generic;

namespace ROS2
{
    /// <summary>
    /// ROS Context encapsulating the non-global state of an init/shutdown cycle.
    /// </summary>
    /// <remarks>
    /// If the instance is not disposed it will be shut down by the garbage collector.
    /// Since the collection tracking the nodes might be finalized at this point
    /// the handle will be leaked.
    /// </remarks>
    public sealed class Context : IContext
    {
        /// <remarks>
        /// Will be disposed on disposal of this instance.
        /// Furthermore, access to the collection is thread safe.
        /// </remarks>
        public IReadOnlyDictionary<string, INode> Nodes { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed { get { return !this.Ok(); } }

        /// <inheritdoc/>
        public event Action OnShutdown;

        /// <summary>
        /// Handle to the <c>rcl_context_t</c>
        /// </summary>
        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Collection nodes active in this context.
        /// </summary>
        /// <remarks>
        /// Also used for synchronisation when creating / removing nodes.
        /// </remarks>
        private Dictionary<string, Node> ROSNodes = new Dictionary<string, Node>();

        /// <summary>
        /// Collection of guard conditions active in this context.
        /// </summary>
        /// <remarks>
        /// Also used for synchronisation when creating / removing guard conditions.
        /// </remarks>
        private HashSet<GuardCondition> GuardConditions = new HashSet<GuardCondition>();

        /// <summary>
        /// Collection of wait sets active in this context;
        /// </summary>
        /// <remarks>
        /// Also used for synchronisation when creating / removing guard conditions.
        /// </remarks>
        private HashSet<WaitSet> WaitSets = new HashSet<WaitSet>();

        /// <summary>
        /// Get the current RMW implementation.
        /// </summary>
        /// <returns>The current implementation as string.</returns>
        public static string GetRMWImplementation()
        {
            return Utils.PtrToString(NativeRmwInterface.rmw_native_interface_get_implementation_identifier());
        }

        /// <summary>
        /// Create a new ROS Context.
        /// </summary>
        public Context()
        {
            this.Nodes = new MappedValueDictionary<string, Node, INode>(
                new LockedDictionary<string, Node>(this.ROSNodes),
                node => node
            );
            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_context();
            int ret = NativeRclInterface.rclcs_init(this.Handle, NativeRcl.rcutils_get_default_allocator());
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        public bool Ok()
        {
            return NativeRclInterface.rclcs_context_is_valid(this.Handle);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        public bool TryCreateNode(string name, out INode node)
        {
            lock (this.ROSNodes)
            {
                if (this.ROSNodes.ContainsKey(name))
                {
                    node = default(INode);
                    return false;
                }
                else
                {
                    Node ROSNode = new Node(name, this);
                    this.ROSNodes.Add(name, ROSNode);
                    node = ROSNode;
                    return true;
                }
            }
        }

        /// <summary>
        /// Remove a Node.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Node.Dispose"/> and does not dispose the node.
        /// Furthermore, it is thread safe.
        /// </remarks>
        /// <param name="name">Name of the node.</param>
        /// <returns>If the node existed in this context and has been removed.</returns>
        internal bool RemoveNode(string name)
        {
            lock (this.ROSNodes)
            {
                return this.ROSNodes.Remove(name);
            }
        }

        /// <summary>
        /// Create a guard condition.
        /// </summary>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <param name="callback"> Callback executed by the executor when the guard condition is triggered. </param>
        /// <returns> A new guard condition instance. </returns>
        internal GuardCondition CreateGuardCondition(Action callback)
        {
            lock (this.GuardConditions)
            {
                GuardCondition guardCondition = new GuardCondition(this, callback);
                this.GuardConditions.Add(guardCondition);
                return guardCondition;
            }
        }

        /// <summary>
        /// Remove a guard condition.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="GuardCondition.Dispose"/> and does not dispose the guard condition.
        /// Furthermore, it is thread safe.
        /// </remarks>
        /// <param name="guardCondition"> Guard condition to remove. </param>
        /// <returns> If the guard condition existed in this context and has been removed. </returns>
        internal bool RemoveGuardCondition(GuardCondition guardCondition)
        {
            lock (this.GuardConditions)
            {
                return this.GuardConditions.Remove(guardCondition);
            }
        }

        /// <summary>
        /// Create a wait set.
        /// </summary>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <returns> A new wait set instance. </returns>
        internal WaitSet CreateWaitSet()
        {
            lock (this.WaitSets)
            {
                WaitSet waitSet = new WaitSet(this);
                this.WaitSets.Add(waitSet);
                return waitSet;
            }
        }

        /// <summary>
        /// Remove a wait set.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="WaitSet.Dispose"/> and does not dispose the wait set.
        /// Furthermore, it is thread safe.
        /// </remarks>
        /// <param name="waitSet"> Wait set to remove. </param>
        /// <returns> If the wait set existed in this context and has been removed. </returns>
        internal bool RemoveWaitSet(WaitSet waitSet)
        {
            lock (this.WaitSets)
            {
                return this.WaitSets.Remove(waitSet);
            }
        }

        /// <remarks>
        /// This method is not thread safe.
        /// Do not call while the context or any entities
        /// associated with it are in use.
        /// </remarks>
        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            // finalizer not needed when we disposed successfully
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposal logic.</summary>
        /// <param name="disposing">If this method is not called in a finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }
            int ret = NativeRcl.rcl_shutdown(this.Handle);
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_ALREADY_SHUTDOWN)
            {
                Utils.CheckReturnEnum(ret);
            }
            // only continue if the collections of the active primitives have not been finalized
            if (disposing)
            {
                this.OnShutdown?.Invoke();
                foreach (var node in this.ROSNodes.Values)
                {
                    node.DisposeFromContext();
                }
                this.ROSNodes.Clear();
                foreach (var guardCondition in this.GuardConditions)
                {
                    guardCondition.DisposeFromContext();
                }
                this.GuardConditions.Clear();
                foreach (var waitSet in this.WaitSets)
                {
                    waitSet.DisposeFromContext();
                }
                this.WaitSets.Clear();
                // only safe when all primitives are gone, not calling Dispose() will leak the Handle
                Utils.CheckReturnEnum(NativeRcl.rcl_context_fini(this.Handle));
                this.FreeHandles();
            }
        }

        /// <summary>
        /// Free the handles of this instance.
        /// </summary>
        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_context(this.Handle);
            // to allow .IsDisposed to work
            this.Handle = IntPtr.Zero;
        }

        ~Context()
        {
            this.Dispose(false);
        }
    }
}
