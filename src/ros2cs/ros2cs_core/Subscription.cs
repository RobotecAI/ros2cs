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
using System.Diagnostics;
using System.Threading.Tasks;
using ROS2.Internal;

namespace ROS2
{
    /// <summary> Subscription to a topic with a given type. </summary>
    /// <inheritdoc cref="ISubscription{T}"/>
    internal sealed class Subscription<T> : ISubscription<T>, IRawSubscription where T : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRcl.rcl_subscription_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        private IntPtr Handle = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private readonly Node Node;

        private readonly Action<T> Callback;

        internal Subscription(string topic, Node node, Action<T> callback, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;
            this.Callback = callback;

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile();

            this.Options = NativeRclInterface.rclcs_subscription_create_options(qualityOfServiceProfile.handle);

            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<T>();

            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_subscription();
            int ret = NativeRcl.rcl_subscription_init(
                this.Handle,
                this.Node.Handle,
                typeSupportHandle,
                this.Topic,
                this.Options
            );
            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_OK)
            {
                this.FreeHandles();
                Utils.CheckReturnEnum(ret);
            }
        }

        /// <inheritdoc/>
        public bool TryProcess()
        {
            if (this.IsDisposed)
            {
                return false;
            }

            T message = new T();
            int ret = NativeRcl.rcl_take(
                this.Handle,
                (message as MessageInternals).Handle,
                IntPtr.Zero,
                IntPtr.Zero
            );
            GC.KeepAlive(this);

            if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_SUBSCRIPTION_TAKE_FAILED)
            {
                Utils.CheckReturnEnum(ret);
                (message as MessageInternals).ReadNativeMessage();
                this.Callback(message);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public Task<bool> TryProcessAsync()
        {
            return Task.FromResult(this.TryProcess());
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

            // only do if Node.CurrentSubscriptions has not been finalized
            if (disposing)
            {
                bool success = this.Node.CurrentSubscriptions.Remove(this);
                Debug.Assert(success, "failed to remove subscription");
                this.Node.Executor?.Wake(this.Node);
            }

            this.DisposeFromNode();
        }

        /// <inheritdoc/>
        public void DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_subscription_fini(this.Handle, this.Node.Handle));
            this.FreeHandles();
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_subscription(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_subscription_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }

        ~Subscription()
        {
            this.Dispose(false);
        }
    }
}
