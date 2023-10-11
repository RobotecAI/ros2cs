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

namespace ROS2
{
    /// <summary>
    /// Object which can process becoming ready.
    /// </summary>
    public interface IWaitable
    {
        /// <summary>
        /// The handle used for adding to a wait set.
        /// </summary>
        IntPtr Handle { get; }

        /// <summary>
        /// Try to process if this instance is ready.
        /// </summary>
        /// <returns> If the instance was ready. </returns>
        bool TryProcess();
    }
}
