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
    /// <remarks>
    /// Instances should be disposed with <see cref="Dispose"/> which may NOT automatically performed completely on garbage collection. 
    /// </remarks>
    public interface IContext : IExtendedDisposable
    {
        /// <summary>
        /// Nodes associated with this instance
        /// </summary>
        /// <remarks>Will be disposed on disposal of this instance.</remarks>
        IReadOnlyDictionary<string, INode> Nodes { get; }

        /// <summary>
        /// Event triggered after context shutdown before disposing nodes and finalization.
        /// </summary>
        event Action OnShutdown;

        /// <summary>
        /// Check if the instance is valid (has not been disposed).
        /// </summary>
        bool Ok();

        /// <summary>
        /// Try to create a <see cref="INode"/>.
        /// </summary>
        /// <param name="name">Name of the node, has to be unqiue</param>
        /// <returns>If the <see cref="INode"/> instance could be created.</returns>
        bool TryCreateNode(string name, out INode node);
    }
}
