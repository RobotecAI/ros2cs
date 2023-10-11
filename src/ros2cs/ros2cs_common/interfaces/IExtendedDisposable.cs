// Copyright 2021 Robotec.ai
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
  /// <summary> Extended Disposable interface to enable dispose check </summary>
  /// <description> Use instead of IDisposable </description>
  public interface IExtendedDisposable : IDisposable
  {
    /// <summary> If the object is in a disposed state. </summary>
    /// <remarks>
    /// Being in a disposed state does not mean that an object has ben disposed successfully.
    /// Call <see cref="Dispose"/> to assert that an object has been disposed successfully.
    /// </remarks>
    bool IsDisposed { get; }
  }

}
