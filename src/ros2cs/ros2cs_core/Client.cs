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
using System.Diagnostics;
using System.Runtime.InteropServices;
using ROS2.Internal;
using example_interfaces;


namespace ROS2
{
  /// <summary> Client of a topic with a given type </summary>
  /// <description> Services are created through INode.CreateClient </description>
  public class Client<T>: IClient<T> where T : Message, new ()
  {
    public string Topic { get { return topic; } }
    private string topic;
    public long sequence_number;
    public bool wait_flag = false;

    private Ros2csLogger logger = Ros2csLogger.GetInstance();
    rcl_client_t serviceHandle;
    IntPtr serviceOptions = IntPtr.Zero;
    rcl_node_t nodeHandle;
    private rcl_rmw_request_id_t request_header = new rcl_rmw_request_id_t();
    private bool disposed = false;

    public bool IsDisposed { get { return disposed; } }

    /// <summary> Internal constructor for Service. Use INode.CreateClient to construct </summary>
    /// <see cref="INode.CreateClient"/>
    public Client(string pubTopic, Node node, QualityOfServiceProfile qos = null)
    {
      topic = pubTopic;
      nodeHandle = node.nodeHandle;

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
        qualityOfServiceProfile = new QualityOfServiceProfile();

      serviceOptions = NativeRclInterface.rclcs_client_create_options(qualityOfServiceProfile.handle);

      IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<T>();

      serviceHandle = NativeRcl.rcl_get_zero_initialized_client();
      Utils.CheckReturnEnum(NativeRcl.rcl_client_init(
                              ref serviceHandle,
                              ref nodeHandle,
                              typeSupportHandle,
                              topic,
                              serviceOptions));
    }

    ~Client()
    {
      Dispose();
    }

    public void Dispose()
    {
      DestroyClient();
    }

    /// <summary> "Destructor" supporting disposable model </summary>
    private void DestroyClient()
    {
      if (!disposed)
      {
        Utils.CheckReturnEnum(NativeRcl.rcl_client_fini(ref serviceHandle, ref nodeHandle));
        NativeRclInterface.rclcs_client_dispose_options(serviceOptions);
        logger.LogInfo("Client destroyed");
        disposed = true;
      }
    }

    /// <summary> Wait Service wakeup </summary>
    /// <see cref="IService.WaitForService"/>
    public void WaitForService(T msg)
    {
      while ( wait_flag == false)
      {
        NativeRcl.rcl_service_server_is_available(ref nodeHandle, ref serviceHandle, ref wait_flag);
        logger.LogInfo("Waiting for server startup");
        System.Threading.Thread.Sleep(1000);
      }
    }

    /// <summary> Service a message </summary>
    /// <see cref="IService.SendAndRecv"/>
    public IntPtr SendAndRecv(T msg)
    ///public long SendAndRecv(T msg)
    {
      IntPtr respp = new IntPtr(0);
      if (!Ros2cs.Ok() || disposed)
      {
        logger.LogWarning("Cannot service as the class is already disposed or shutdown was called");
        return(respp);
      }
      MessageInternals msgInternals = msg as MessageInternals;
      msgInternals.WriteNativeMessage();

      /// send request
      Utils.CheckReturnEnum(NativeRcl.rcl_send_request(ref serviceHandle, msgInternals.Handle, ref sequence_number));
      System.Threading.Thread.Sleep(1000);
      Utils.CheckReturnEnum(NativeRcl.rcl_take_response(ref serviceHandle, ref request_header, ref respp));
      return(respp);
    }
  }
}
