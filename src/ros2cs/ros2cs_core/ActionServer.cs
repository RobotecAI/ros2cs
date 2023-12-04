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
using action_msgs.srv;
using action_msgs.msg;

namespace ROS2
{

  /// <summary> Enum to indicate handling of new goal </summary>
  public enum ActionGoalResponse
  {
    REJECT,
    ACCEPT_AND_EXECUTE,
    ACCEPT_AND_DEFER
  }

  public class ActionServer<TGoalRequest, TGoalResponse, TFeedback, TResultRequest, TResultResponse>: IActionServer<TGoalRequest, TGoalResponse, TFeedback, TResultRequest, TResultResponse>
    where TGoalRequest : Message, new()
    where TGoalResponse : Message, new()
    where TFeedback : Message, new()
    where TResultRequest : Message, new()
    where TResultResponse : Message, new()
  {

    /// <summary> Service to cancel action (same for every action) </summary>
    private Service<CancelGoal_Request, CancelGoal_Response> serviceCancel;

    /// <summary> Service to reply to a result request </summary>
    private Service<TResultRequest, TResultResponse> serviceResult;

    /// <summary> Service to register a new goal </summary>
    private Service<TGoalRequest, TGoalResponse> serviceGoal;

    /// <summary> Topic to which action status is published (same for every action) </summary>
    private Publisher<GoalStatusArray> publisherStatus;

    /// <summary> Topic to which action progress is published </summary>
    private Publisher<TFeedback> publisherFeedback;

    /// <summary>
    /// Internal constructor
    /// </summary>
    /// <remarks>Use <see cref="INode.CreateActionServer"/> to construct new Instances</remarks>
    internal ActionServer(
      string topic,
      Node node,
      Func<TGoalRequest, ActionGoalResponse> handleGoal,
      Func<CancelGoal_Request, CancelGoal_Response> handleCancel,
      Action<TGoalRequest> handleAccepted
    )
    {
      string prefix = topic + "_action/";

      QualityOfServiceProfile qos_service = new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);
      QualityOfServiceProfile qos_status = new QualityOfServiceProfile(QosPresetProfile.ACTION_STATUS_DEFAULT);

      // Default service and topic:

      this.serviceCancel = node.CreateService<CancelGoal_Request, CancelGoal_Response>(
        prefix + "cancel_goal",
        handleCancel,
        qos_service
      );

      this.publisherStatus = node.CreatePublisher<GoalStatusArray>(
        prefix + "status",
        qos_status
      );

      // Custom:

      this.serviceGoal = node.CreateService<TGoalRequest, TGoalResponse>(
        prefix + "send_goal",
        onGoalReceive,
        qos_service
      );

      this.serviceResult = node.CreateService<TResultRequest, TResultResponse>(
        prefix + "get_result",
        onResultReceive,
        qos_service
      );
    }

    public void Dispose()
    {
      DestroyActionServer();
    }

    /// <summary>
    /// Callback for the goal service
    ///
    /// Most of the logic will be in the user defined function, but we wrap it here.
    /// </summary>
    private TGoalResponse onGoalReceive(TGoalRequest request)
    {
      TGoalResponse response = new TGoalResponse();
      return response;
    }

    private TResultResponse onResultReceive(TResultRequest request)
    {
      TResultResponse response = new TResultResponse();
      return response;
    }

    /// <summary>
    /// Callback for cancel service
    /// </summary>
    private CancelGoal_Response defaultOnCancelReceive(CancelGoal_Request request)
    {
      CancelGoal_Response response = new CancelGoal_Response()
      {
        Return_code = CancelGoal_Response.ERROR_REJECTED
      };
      return response;
    }


    /// <inheritdoc/>
    public bool IsDisposed { get { return IsDisposed; } }
    private bool disposed = false;

    private void DestroyActionServer()
    {

    }
  }
}
