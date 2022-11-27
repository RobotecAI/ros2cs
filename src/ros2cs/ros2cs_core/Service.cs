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
  /// <summary> Service to a topic with a given type </summary>
  /// <description> Services are created through INode interface (CreateService) </description>
  public class Service<I, O>: IService<I, O>
    where I : Message, new ()
    where O : Message, new ()
  {
    public rcl_service_t Handle { get { return serviceHandle; } }
    private rcl_service_t serviceHandle;

    public string Topic { get { return topic; } }
    private string topic;

    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    private rcl_node_t nodeHandle;
    private readonly Func<I, O> callback;
    private IntPtr serviceOptions;

    public object Mutex { get { return mutex; } }
    private object mutex = new object();

    /// <summary> Tries to send a response message to rcl/rmw layers. </summary>
    // TODO(adamdbrw) this should not be public - add an internal interface
    private void SendResp(rcl_rmw_request_id_t header, O msg)
    {
      RCLReturnEnum ret;
      MessageInternals msgInternals = msg as MessageInternals;

      msgInternals.WriteNativeMessage();
      ret = (RCLReturnEnum)NativeRcl.rcl_send_response(ref serviceHandle, ref header, msgInternals.Handle);
    }

    /// <summary> Tries to get a request message from rcl/rmw layers. Calls the callback if successful </summary>
    // TODO(adamdbrw) this should not be public - add an internal interface
    public void TakeMessage()
    {
      RCLReturnEnum ret;
      rcl_rmw_request_id_t header = default;
      MessageInternals message;

      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          return;
        }
        message = CreateMessage();

        ret = (RCLReturnEnum)NativeRcl.rcl_take_request(ref serviceHandle, ref header,  message.Handle);
      }

      bool gotMessage = ret == RCLReturnEnum.RCL_RET_OK;

      if (gotMessage)
      {
        TriggerCallback(header, message);
      }
    }

    /// <summary> Construct a message of the subscription type </summary>
    private MessageInternals CreateMessage()
    {
      return new I() as MessageInternals;
    }

    /// <summary> Populates managed fields with native values and calls the callback with created message </summary>
    /// <param name="message"> Message that will be populated and returned through callback </param>
    private void TriggerCallback(rcl_rmw_request_id_t header, MessageInternals message)
    {
      message.ReadNativeMessage();
      O response = callback((I)message);
      SendResp(header, response);
    }

    /// <summary> Internal constructor for Service. Use INode.CreateService to construct </summary>
    /// <see cref="INode.CreateService"/>
    internal Service(string subTopic, Node node, Func<I, O> cb, QualityOfServiceProfile qos = null)
    {
      callback = cb;
      nodeHandle = node.nodeHandle;
      topic = subTopic;
      serviceHandle = NativeRcl.rcl_get_zero_initialized_service();

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
      {
        qualityOfServiceProfile = new QualityOfServiceProfile();
      }

      serviceOptions = NativeRclInterface.rclcs_service_create_options(qualityOfServiceProfile.handle);

      I msg = new I();
      MessageInternals msgInternals = msg as MessageInternals;
      IntPtr typeSupportHandle = msgInternals.TypeSupportHandle;
      msg.Dispose();

      Utils.CheckReturnEnum(NativeRcl.rcl_service_init(
        ref serviceHandle,
        ref node.nodeHandle,
        typeSupportHandle,
        topic,
        serviceOptions));
    }

    ~Service()
    {
      DestroyService();
    }

    public void Dispose()
    {
      DestroyService();
    }

    /// <summary> "Destructor" supporting disposable model </summary>
    private void DestroyService()
    {
      lock (mutex)
      {
        if (!disposed)
        {
          Utils.CheckReturnEnum(NativeRcl.rcl_service_fini(ref serviceHandle, ref nodeHandle));
          NativeRclInterface.rclcs_node_dispose_options(serviceOptions);
          disposed = true;
          Ros2csLogger.GetInstance().LogInfo("Service destroyed");
        }
      }
    }
  }
}
