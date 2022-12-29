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

using System.Collections.Generic;

namespace ROS2
{
    /// <summary>
    /// Executor controlling the processing of callbacks of some nodes.
    /// </summary>
    /// <remarks>Adding and removing Nodes should be done by calling <see cref="INode.TrySetExecutor"/>.</remarks>
    public interface IExecutor: IExtendedDisposable, ICollection<INode>
    {
        /// <summary>
        /// Notify the instance that some nodes changed.
        /// </summary>
        /// <remarks>
        /// This is used to tell the executor when entities are created or destroyed.
        /// The executor has to guarantee that removed entities can be
        /// disposed after this method returns.
        /// </remarks>
        void Wake();

        /// <summary>
        /// Notify the instance that a node changed or got added or removed.
        /// </summary>
        /// <param name="node">Node which changed and is handled by the executor.</param>
        /// <returns>If the node is handled by the executor.</returns>
        /// <inheritdoc cref="Wake"/>
        bool Wake(INode node);
    }
}
