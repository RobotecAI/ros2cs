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
    /// <summary>
    /// Publisher of a topic with a given type wrapping a rcl publisher.
    /// </summary>
    /// <remarks>
    /// This is the implementation produced by <see cref="Node.CreatePublisher"/>,
    /// use this method to create new instances.
    /// </remarks>
    /// <seealso cref="ROS2.Node"/>
    /// <inheritdoc cref="IPublisher{T}"/>
    public sealed class Publisher<T> : IPublisher<T>, IRawPublisher where T : Message, new()
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

        /// <summary>
        /// Handle to the rcl publisher
        /// </summary>
        private IntPtr Handle = IntPtr.Zero;

        /// <summary>
        /// Handle to the rcl publisher options
        /// </summary>
        private IntPtr Options = IntPtr.Zero;

        /// <summary>
        /// Node associated with this instance.
        /// </summary>
        private readonly Node Node;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for adding the instance to <paramref name="node"/>.
        /// This action is not thread safe.
        /// </remarks>
        /// <param name="topic"> Topic to publish to. </param>
        /// <param name="node"> Node to associate with. </param>
        /// <param name="qos"> QOS setting for this publisher. </param>
        /// <exception cref="ObjectDisposedException"> If <paramref name="node"/> was disposed. </exception>
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


        ///<remarks>
        /// Message memory is copied into native structures and
        /// the message can be safely changed or disposed after this call.
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously.
        /// </remarks>
        /// <exception cref="ObjectDisposedException"> If the instance was disposed. </exception>
        /// <inheritdoc/>
        public void Publish(T msg)
        {
            MessageInternals msgInternals = msg as MessageInternals;
            // may not be thread safe
            msgInternals.WriteNativeMessage();
            // confused by the rcl documentation, assume it is not thread safe
            Utils.CheckReturnEnum(NativeRcl.rcl_publish(this.Handle, msgInternals.Handle, IntPtr.Zero));
            GC.KeepAlive(this);
        }

        /// <remarks>
        /// This method is not thread safe and may not be called from
        /// multiple threads simultaneously or while the publisher is in use.
        /// Disposal is automatically performed on finalization by the GC.
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

            // only do if Node.CurrentPublishers has not been finalized
            if (disposing)
            {
                bool success = this.Node.RemovePublisher(this);
                Debug.Assert(success, "failed to remove publisher");
            }

            (this as IRawPublisher).DisposeFromNode();
        }

        /// <inheritdoc/>
        void IRawPublisher.DisposeFromNode()
        {
            if (this.Handle == IntPtr.Zero)
            {
                return;
            }

            Utils.CheckReturnEnum(NativeRcl.rcl_publisher_fini(this.Handle, this.Node.Handle));
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
