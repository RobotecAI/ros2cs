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

namespace ROS2
{
  public interface IActionServerBase : IExtendedDisposable
  {

  }

  /// <summary> Generic base interface for all actions (either server or client) </summary>
  public interface IActionServer<TGoalRequest, TGoalResponse, TFeedback, TResultRequest, TResultResponse> : IActionServerBase
    where TGoalRequest : Message, new()
    where TGoalResponse : Message, new()
    where TFeedback : Message, new()
    where TResultRequest : Message, new()
    where TResultResponse : Message, new()
  {
  }
}
