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
using System.Diagnostics;

namespace ROS2
{
    /// <summary>
    /// Guard condition used to interrupt waits wrapping a rcl guard condition.
    /// </summary>
    internal sealed class GuardCondition : IWaitable, IExtendedDisposable {

        /// <summary>
        /// Handle to the rcl guard condition.
        /// </summary>
        public IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRclInterface.rclcs_guard_condition_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        /// <summary>
        /// Context associated with this instance.
        /// </summary>
        private readonly Context Context;

        /// <summary>
        /// Callback invoked when the guard condition
        /// is processed.
        /// </summary>
        private readonly Action Callback;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="context"> Context to associate with. </param>
        /// <param name="callback"> Callback to invoke when processed. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="context"/> is disposed. </exception>
        internal GuardCondition(Context context, Action callback)
        {
            this.Context = context;
            this.Callback = callback;
            int ret = NativeRclInterface.rclcs_get_guard_condition(
                context.Handle,
                out IntPtr handle
            );
            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_INVALID_ARGUMENT)
            {
                throw new ObjectDisposedException("rcl context");
            }
            Utils.CheckReturnEnum(ret);
            this.Handle = handle;
        }

        /// <summary>
        /// Trigger the guard condition to make it become ready.
        /// </summary>
        /// <remarks>
        /// It seems that the guard condition stays ready until waited on.
        /// This method is thread safe.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">If the guard condition was disposed.</exception>
        public void Trigger()
        {
            int ret = NativeRcl.rcl_trigger_guard_condition(this.Handle);
            GC.KeepAlive(this);

            if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_INVALID_ARGUMENT)
            {
                throw new ObjectDisposedException("rcl guard condition");
            }
            Utils.CheckReturnEnum(ret);
        }

        /// <remarks>
        /// This method is thread safe
        /// is the callback is thread safe.
        /// </remarks>
        /// <inheritdoc/>
        public bool TryProcess()
        {
            this.Callback();
            return true;
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the guard condition is in use.
        /// Disposal is automatically performed on finalization by the GC.
        /// </remarks>
        /// <inheritdoc/>
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

            // only do if Context.GuardConditions has not been finalized
            if (disposing)
            {
                bool success = this.Context.RemoveGuardCondition(this);
                Debug.Assert(success, message: "failed to remove guard condition");
            }

            this.DisposeFromContext();
        }

        /// <summary> Dispose without modifying the context. </summary>
        internal void DisposeFromContext()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_guard_condition_fini(this.Handle));
            this.FreeHandles();
        }

        /// <summary>
        /// Free the rcl handles and replace them with null pointers.
        /// </summary>
        /// <remarks>
        /// The handles are not finalised by this method.
        /// </remarks>
        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_guard_condition(this.Handle);
            this.Handle = IntPtr.Zero;
        }

        ~GuardCondition()
        {
            this.Dispose(false);
        }
    }
}