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
    /// <summary>Service with a topic and Types for Messages</summary>
    /// <remarks>Instances are created by <see cref="INode.CreateService"/></remarks>
    /// <typeparam name="I">Message Type to be received</typeparam>
    /// <typeparam name="O">Message Type to be send</typeparam>
    public class Service<I, O>: IService<I, O>
    where I : Message, new ()
    where O : Message, new ()
  {
    public rcl_service_t Handle { get { return serviceHandle; } }
    private rcl_service_t serviceHandle;

    /// <summary>
    /// Topic of this Service
    /// </summary>
    public string Topic { get { return topic; } }
    private string topic;

    /// <inheritdoc/>
    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    /// <inheritdoc/>
    private rcl_node_t nodeHandle;

    /// <summary>
    /// Callback to be called to process incoming requests
    /// </summary>
    private readonly Func<I, O> callback;
    private IntPtr serviceOptions;

    /// <inheritdoc/>
    public object Mutex { get { return mutex; } }
    private object mutex = new object();

    /// <summary>
    /// Internal constructor for Service
    /// </summary>
    /// <remarks>Use <see cref="INode.CreateService"/> to construct new Instances</remarks>
    internal Service(string subTopic, Node node, Func<I, O> cb, QualityOfServiceProfile qos = null)
    {
      callback = cb;
      nodeHandle = node.nodeHandle;
      topic = subTopic;
      serviceHandle = NativeRcl.rcl_get_zero_initialized_service();

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
      {
        qualityOfServiceProfile = new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);
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

    /// <summary>
    /// Send Response Message with rcl/rmw layers
    /// </summary>
    /// <param name="header">request id received when taking the Request</param>
    /// <param name="msg">Message to be send</param>
    private void SendResp(rcl_rmw_request_id_t header, O msg)
    {
      RCLReturnEnum ret;
      MessageInternals msgInternals = msg as MessageInternals;
      msgInternals.WriteNativeMessage();
      ret = (RCLReturnEnum)NativeRcl.rcl_send_response(ref serviceHandle, ref header, msgInternals.Handle);
    }

    /// <inheritdoc/>
    // TODO(adamdbrw) this should not be public - add an internal interface
    public void TakeMessage()
    {
      RCLReturnEnum ret;
      rcl_rmw_request_id_t header = default(rcl_rmw_request_id_t);
      MessageInternals message;

      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          return;
        }
        message = new I() as MessageInternals;

        ret = (RCLReturnEnum)NativeRcl.rcl_take_request(ref serviceHandle, ref header,  message.Handle);
      }

      if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_OK)
      {
        ProcessRequest(header, message);
      }
    }

    /// <summary>
    /// Populates managed fields with native values and calls the callback with the created message
    /// </summary>
    /// <remarks>Sending the Response is also takes care of by this method</remarks>
    /// <param name="message">Message that will be populated and provided to the callback</param>
    /// <param name="header">request id received when taking the Request</param>
    private void ProcessRequest(rcl_rmw_request_id_t header, MessageInternals message)
    {
      message.ReadNativeMessage();
      O response = callback((I)message);
      SendResp(header, response);
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
