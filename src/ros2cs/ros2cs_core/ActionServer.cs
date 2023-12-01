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
using ROS2.Internal;

namespace ROS2
{
  public class ActionServer<TGoalRequest, TGoalResponse, TFeedback, TResultRequest, TResultResponse>: IActionServer<TGoalRequest, TGoalResponse, TFeedback, TResultRequest, TResultResponse>
    where TGoalRequest : Message, new()
    where TGoalResponse : Message, new()
    where TFeedback : Message, new()
    where TResultRequest : Message, new()
    where TResultResponse : Message, new()
  {
    /// <summary>
    /// Internal constructor
    /// </summary>
    /// <remarks>Use <see cref="INode.CreateActionServer"/> to construct new Instances</remarks>
    internal ActionServer(string subTopic, Node node, Func<TGoalRequest, TGoalResponse> cb, QualityOfServiceProfile qpos = null)
    {

    }

    public void Dispose()
    {
      DestroyActionServer();
    }

    /// <inheritdoc/>
    public bool IsDisposed { get { return IsDisposed; } }
    private bool disposed = false;

    private void DestroyActionServer()
    {

    }
  }
}
