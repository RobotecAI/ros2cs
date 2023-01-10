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
using ROS2.Internal;

namespace ROS2
{
    /// <summary> Publisher of a topic with a given type. </summary>
    /// <inheritdoc cref="IPublisher{T}"/>
    internal sealed class Publisher<T> : IPublisher<T>, IRawPublisher where T : Message, new()
    {
        /// <inheritdoc/>
        public string Topic { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed
        {
            get
            {
                bool ok = NativeRclInterface.rclcs_publisher_is_valid(this.Handle);
                GC.KeepAlive(this);
                return !ok;
            }
        }

        private IntPtr Handle = IntPtr.Zero;

        private IntPtr Options = IntPtr.Zero;

        private readonly Node Node;

        internal Publisher(string topic, Node node, QualityOfServiceProfile qos = null)
        {
            this.Topic = topic;
            this.Node = node;

            QualityOfServiceProfile qualityOfServiceProfile = qos ?? new QualityOfServiceProfile();

            this.Options = NativeRclInterface.rclcs_publisher_create_options(qualityOfServiceProfile.handle);

            IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<T>();

            this.Handle = NativeRclInterface.rclcs_get_zero_initialized_publisher();
            int ret = NativeRcl.rcl_publisher_init(
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

        /// <summary>
        /// Assert that the publisher has not been disposed.
        /// </summary>
        private void AssertOk()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException($"publisher for topic '{this.Topic}'");
            }
        }

        /// <inheritdoc/>
        public void Publish(T msg)
        {
            this.AssertOk();
            MessageInternals msgInternals = msg as MessageInternals;
            msgInternals.WriteNativeMessage();
            Utils.CheckReturnEnum(NativeRcl.rcl_publish(this.Handle, msgInternals.Handle, IntPtr.Zero));
            GC.KeepAlive(this);
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

            // only do if Node.CurrentPublishers has not been finalized
            if (disposing)
            {
                bool success = this.Node.RemovePublisher(this);
                Debug.Assert(success, "failed to remove publisher");
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

            Utils.CheckReturnEnum(NativeRcl.rcl_publisher_fini(this.Handle, this.Node.Handle));
            this.FreeHandles();
        }

        private void FreeHandles()
        {
            NativeRclInterface.rclcs_free_publisher(this.Handle);
            this.Handle = IntPtr.Zero;
            NativeRclInterface.rclcs_publisher_dispose_options(this.Options);
            this.Options = IntPtr.Zero;
        }

        ~Publisher()
        {
            this.Dispose(false);
        }
    }
}
