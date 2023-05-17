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
    /// Executor controlling the processing of callbacks of a set of nodes.
    /// </summary>
    /// <remarks>
    /// Adding and removing Nodes has to update <see cref="INode.Executor"/>.
    /// Furthermore, removing Nodes has to guarantee that the Node is ready to be disposed.
    /// </remarks>
    public interface IExecutor: IExtendedDisposable, ICollection<INode>
    {
        /// <summary>
        /// Notify the instance that some nodes changed.
        /// </summary>
        /// <remarks>
        /// This is used to tell the executor when entities are created or destroyed.
        /// </remarks>
        void ScheduleRescan();

        /// <summary>
        /// Notify the instance that a node changed.
        /// </summary>
        /// <param name="node">Node which changed.</param>
        /// <returns>If a rescan was scheduled.</returns>
        /// <inheritdoc cref="ScheduleRescan"/>
        bool TryScheduleRescan(INode node);

        /// <summary>
        /// Wait for scheduled rescans to complete.
        /// </summary>
        /// <remarks>
        /// This is used for example to ensure that removed objects
        /// are removed from the executor before they are disposed.
        /// Return immediately if no rescans are scheduled.
        /// </remarks>
        void Wait();

        /// <param name="timeout">positive Amount of time to wait.</param>
        /// <returns>Wether no timeout occurred.</returns>
        /// <inheritdoc cref="Wait"/>
        bool TryWait(TimeSpan timeout);
    }
}
