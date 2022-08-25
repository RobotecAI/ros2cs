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
  public class Service<T>: IService<T> where T : Message, new ()
  {
    public rcl_service_t Handle { get { return serviceHandle; } }
    private rcl_service_t serviceHandle;

    public string Topic { get { return topic; } }
    private string topic;

    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    private rcl_node_t nodeHandle;
    private readonly Action<T> callback;
    private IntPtr serviceOptions;

    public object Mutex { get { return mutex; } }
    private object mutex = new object();

    private rcl_rmw_request_id_t request_header;

    /// <summary> Tries to send a response message to rcl/rmw layers. </summary>
    // TODO(adamdbrw) this should not be public - add an internal interface
    public void SendResp(IntPtr valp)
    {
      RCLReturnEnum ret;

      ret = (RCLReturnEnum)NativeRcl.rcl_send_response(ref serviceHandle, ref request_header,  ref valp);
    }

    /// <summary> Tries to get a request message from rcl/rmw layers. Calls the callback if successful </summary>
    // TODO(adamdbrw) this should not be public - add an internal interface
    public void TakeMessage()
    {
      RCLReturnEnum ret;
      MessageInternals message;

      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          return;
        }
        message = CreateMessage();

	ret = (RCLReturnEnum)NativeRcl.rcl_take_request(ref serviceHandle, ref request_header,  message.Handle);
      }

      bool gotMessage = ret == RCLReturnEnum.RCL_RET_OK;

      if (gotMessage)
      {
        TriggerCallback(message);
      }
    }

    /// <summary> Construct a message of the subscription type </summary>
    private MessageInternals CreateMessage()
    {
      return new T() as MessageInternals;
    }

    /// <summary> Populates managed fields with native values and calls the callback with created message </summary>
    /// <param name="message"> Message that will be populated and returned through callback </param>
    private void TriggerCallback(MessageInternals message)
    {
      message.ReadNativeMessage();
      callback((T)message);
    }

    /// <summary> Internal constructor for Service. Use INode.CreateService to construct </summary>
    /// <see cref="INode.CreateService"/>
    internal Service(string subTopic, Node node, Action<T> cb, QualityOfServiceProfile qos = null)
    {
      callback = cb;
      nodeHandle = node.nodeHandle;
      topic = subTopic;
      serviceHandle = NativeRcl.rcl_get_zero_initialized_service();
      request_header = new rcl_rmw_request_id_t();

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
      {
        qualityOfServiceProfile = new QualityOfServiceProfile();
      }

      serviceOptions = NativeRclInterface.rclcs_service_create_options(qualityOfServiceProfile.handle);

      T msg = new T();
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
