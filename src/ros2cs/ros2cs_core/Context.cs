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
using System.Collections.Generic;

namespace ROS2
{
    /// <summary>
    /// ROS Context encapsulating the non-global state of an init/shutdown cycle.
    /// </summary>
    /// <remarks>Can be disposed by the garbage collector</remarks>
    public sealed class Context : IContext
    {
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, INode> Nodes { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed { get { return !this.Ok(); } }

        /// <inheritdoc/>
        public event Action OnShutdown;

        internal IntPtr Handle { get; private set; } = IntPtr.Zero;

        private Dictionary<string, Node> ROSNodes = new Dictionary<string, Node>();

        private rcl_allocator_t ROSAllocator;

        /// <summary>
        /// Get the current RMW implementation.
        /// </summary>
        /// <returns>The current implementation as string.</returns>
        public static string GetRMWImplementation()
        {
            return Utils.PtrToString(NativeRmwInterface.rmw_native_interface_get_implementation_identifier());
        }

        public Context()
        {
            this.Nodes = new MappingValueView<string, Node, INode>(this.ROSNodes, node => node);
            this.ROSAllocator = NativeRcl.rcutils_get_default_allocator();
            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_context();
            int ret = NativeRclInterface.rclcs_init(this.Handle, this.ROSAllocator);
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
        }

        /// <summary>
        /// Assert that the context has not been disposed.
        /// </summary>
        private void AssertOk()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("current ROS context");
            }
        }

        /// <inheritdoc/>
        public bool Ok()
        {
            return NativeRcl.rcl_context_is_valid(this.Handle);
        }

        /// <inheritdoc/>
        public bool TryCreateNode(string name, out INode node)
        {
            this.AssertOk();
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

        /// <summary>
        /// Remove a Node.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used by <see cref="Node.Dispose"/> and does not dispose the node.
        /// </remarks>
        /// <param name="name">Name of the node.</param>
        /// <returns>If the node existed in this context and has been removed.</returns>
        internal bool RemoveNode(string name)
        {
            return this.ROSNodes.Remove(name);
        }

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
            // only continue if ROSNodes has not been finalized
            if (disposing)
            {
                this.OnShutdown?.Invoke();
                foreach (var node in this.ROSNodes.Values)
                {
                    node.DisposeFromContext();
                }
                this.ROSNodes.Clear();
                // only safe when all nodes are gone, not calling Dispose() will leak the Handle
                Utils.CheckReturnEnum(NativeRcl.rcl_context_fini(this.Handle));
                this.FreeHandles();
            }
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_context(this.Handle);
            this.Handle = IntPtr.Zero;
        }

        ~Context()
        {
            this.Dispose(false);
        }
    }
}
