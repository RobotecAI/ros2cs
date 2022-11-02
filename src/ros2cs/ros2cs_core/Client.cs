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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using ROS2.Internal;


namespace ROS2
{
  /// <summary> Client of a topic with a given type </summary>
  /// <description> Services are created through INode.CreateClient </description>
  public class Client<I, O>: IClient<I, O>
    where I : Message, new()
    where O : Message, new()
  {
    public string Topic { get { return topic; } }

    public rcl_client_t Handle { get { return serviceHandle; } }

    private string topic;

    public object Mutex { get { return mutex; } }

    private object mutex = new object();

    private Dictionary<long, Action<O>> Requests;

    private Ros2csLogger logger = Ros2csLogger.GetInstance();
    rcl_client_t serviceHandle;
    IntPtr serviceOptions = IntPtr.Zero;
    rcl_node_t nodeHandle;

    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    public static readonly int ST_RETRY_COUNT = 10;

    /// <summary> Internal constructor for Service. Use INode.CreateClient to construct </summary>
    /// <see cref="INode.CreateClient"/>
    public Client(string pubTopic, Node node, QualityOfServiceProfile qos = null)
    {
      topic = pubTopic;
      nodeHandle = node.nodeHandle;

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
        qualityOfServiceProfile = new QualityOfServiceProfile();

      Requests = new Dictionary<long, Action<O>>();

      serviceOptions = NativeRclInterface.rclcs_client_create_options(qualityOfServiceProfile.handle);

      IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<I>();

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
    public void WaitForService()
    {
      bool wait_flag = false;
      while (!wait_flag)
      {
        int ret = NativeRcl.rcl_service_server_is_available(
          ref nodeHandle,
          ref serviceHandle,
          ref wait_flag
        );
        Utils.CheckReturnEnum(ret);
        logger.LogInfo("Waiting for server startup");
        System.Threading.Thread.Sleep(1000);
      }
    }

    public void TakeMessage()
    {
      MessageInternals msg = new O() as MessageInternals;

      while (true)
      {
        rcl_rmw_request_id_t request_header = default;
        int ret = NativeRcl.rcl_take_response(
          ref serviceHandle,
          ref request_header,
          msg.Handle
        );
        if ((RCLReturnEnum)ret == RCLReturnEnum.RCL_RET_CLIENT_TAKE_FAILED)
        {
          break;
        }
        else
        {
          Utils.CheckReturnEnum(ret);
          HandleResponse(request_header.sequence_number, msg);
          msg = new O() as MessageInternals;
        }
      }
    }

    private void HandleResponse(long sequence_number, MessageInternals msg)
    {
      bool exists = default;
      Action<O> action = default;
      lock (Requests)
      {
        exists = Requests.Remove(sequence_number, out action);
      }
      Debug.Assert(exists, "received invalid sequence number");
      msg.ReadNativeMessage();
      action((O)msg);
    }

    private long SendRequest(I msg)
    {
      long sequence_number = default;
      MessageInternals msgInternals = msg as MessageInternals;
      msgInternals.WriteNativeMessage();
      Utils.CheckReturnEnum(
        NativeRcl.rcl_send_request(
          ref serviceHandle,
          msgInternals.Handle,
          ref sequence_number
        )
      );
      return sequence_number;
    }

    private void RegisterSource(TaskCompletionSource<O> source, long sequence_number)
    {
      lock (Requests)
      {
        Requests.Add(sequence_number, source.SetResult);
      }
    }

    public Task<O> SendAndRecv(I msg)
    {
      if (!Ros2cs.Ok() || disposed)
      {
        throw new InvalidOperationException("Cannot service as the class is already disposed or shutdown was called");
      }
      long sequence_number = SendRequest(msg);
      TaskCompletionSource<O> source = new TaskCompletionSource<O>();
      RegisterSource(source, sequence_number);
      return source.Task;
    }

    public Task<O> SendAndRecv(I msg, TaskCreationOptions options)
    {
      if (!Ros2cs.Ok() || disposed)
      {
        throw new InvalidOperationException("Cannot service as the class is already disposed or shutdown was called");
      }
      long sequence_number = SendRequest(msg);
      TaskCompletionSource<O> source = new TaskCompletionSource<O>(options);
      RegisterSource(source, sequence_number);
      return source.Task;
    }
  }
}
