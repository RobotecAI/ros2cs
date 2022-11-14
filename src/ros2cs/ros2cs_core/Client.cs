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
using System.Diagnostics;
using ROS2.Internal;


namespace ROS2
{
  /// <summary>Client with a topic and Types for Messages</summary>
  /// <remarks>Instances are created by <see cref="INode.CreateClient"/></remarks>
  /// <typeparam name="I">Message Type to be send</typeparam>
  /// <typeparam name="O">Message Type to be received</typeparam>
  public class Client<I, O>: IClient<I, O>
    where I : Message, new()
    where O : Message, new()
  {
    /// <inheritdoc/>
    public string Topic { get { return topic; } }

    public rcl_client_t Handle { get { return serviceHandle; } }

    private string topic;

    /// <inheritdoc/>
    public object Mutex { get { return mutex; } }

    private object mutex = new object();

    /// <summary>
    /// Mapping from request id without Response to <see cref="Task"/>
    /// </summary>
    private Dictionary<long, TaskCompletionSource<O>> Requests;

    private Ros2csLogger logger = Ros2csLogger.GetInstance();

    rcl_client_t serviceHandle;

    IntPtr serviceOptions = IntPtr.Zero;

    rcl_node_t nodeHandle;

    /// <inheritdoc/>
    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    /// <summary>
    /// Internal constructor for Client
    /// </summary>
    /// <remarks>Use <see cref="INode.CreateClient"/> to construct new Instances</remarks>
    public Client(string pubTopic, Node node, QualityOfServiceProfile qos = null)
    {
      topic = pubTopic;
      nodeHandle = node.nodeHandle;

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
        qualityOfServiceProfile = new QualityOfServiceProfile(QosPresetProfile.SERVICES_DEFAULT);

      Requests = new Dictionary<long, TaskCompletionSource<O>>();

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
      lock (mutex)
      {
        if (!disposed)
        {
          lock (Requests)
          {
            foreach (var source in Requests.Values)
            {
              bool success = source.TrySetException(new ObjectDisposedException("client has been disposed"));
              Debug.Assert(success);
            }
            Requests.Clear();
          }
          Utils.CheckReturnEnum(NativeRcl.rcl_client_fini(ref serviceHandle, ref nodeHandle));
          NativeRclInterface.rclcs_client_dispose_options(serviceOptions);
          logger.LogInfo("Client destroyed");
          disposed = true;
        }
      }
    }

    /// <inheritdoc/>
    public bool IsServiceAvailable()
    {
      bool available = false;
      Utils.CheckReturnEnum(NativeRcl.rcl_service_server_is_available(
        ref nodeHandle,
        ref serviceHandle,
        ref available
      ));
      return available;
    }

    /// <inheritdoc/>
    public void TakeMessage()
    {
      MessageInternals msg = new O() as MessageInternals;
      rcl_rmw_request_id_t request_header = default;
      int ret;
      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          return;
        }
        ret = NativeRcl.rcl_take_response(
          ref serviceHandle,
          ref request_header,
          msg.Handle
        );
      }
      if ((RCLReturnEnum)ret != RCLReturnEnum.RCL_RET_CLIENT_TAKE_FAILED)
      {
        Utils.CheckReturnEnum(ret);
        ProcessResponse(request_header.sequence_number, msg);
      }
    }

    /// <summary>
    /// Populates managed fields with native values and finishes the corresponding <see cref="Task"/> 
    /// </summary>
    /// <param name="message">Message that will be populated and used as the task result</param>
    /// <param name="header">sequence number received when sending the Request</param>
    private void ProcessResponse(long sequence_number, MessageInternals msg)
    {
      bool exists = default;
      TaskCompletionSource<O> source = default;
      lock (Requests)
      {
        exists = Requests.Remove(sequence_number, out source);
      }
      if (exists)
      {
        msg.ReadNativeMessage();
        source.SetResult((O)msg);
      }
      else
      {
        Debug.Print("received unknown sequence number or got disposed");
      }
    }

    /// <summary>
    /// Send a Request to the Service
    /// </summary>
    /// <param name="msg">Message to be send</param>
    /// <returns>sequence number of the Request</returns>
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

    /// <summary>
    /// Associate a task with a sequence number
    /// </summary>
    /// <param name="source">source used to controll the <see cref="Task"/></param>
    /// <param name="sequence_number">sequence number received when sending the Request</param>
    private void RegisterSource(TaskCompletionSource<O> source, long sequence_number)
    {
      lock (Requests)
      {
        Requests.Add(sequence_number, source);
      }
    }

    /// <inheritdoc/>
    public O Call(I msg)
    {
      var task = CallAsync(msg);
      task.Wait();
      return task.Result;
    }

    /// <inheritdoc/>
    public Task<O> CallAsync(I msg)
    {
      return CallAsync(msg, TaskCreationOptions.None);
    }

    /// <inheritdoc/>
    public Task<O> CallAsync(I msg, TaskCreationOptions options)
    {
      TaskCompletionSource<O> source;
      lock (mutex)
      {
          if (!Ros2cs.Ok() || disposed)
          {
            throw new InvalidOperationException("Cannot service as the class is already disposed or shutdown was called");
          }
          // prevent TakeMessage from receiving Responses before we called RegisterSource
          long sequence_number = SendRequest(msg);
          source = new TaskCompletionSource<O>(options);
          RegisterSource(source, sequence_number);
      }
      return source.Task;
    }
  }
}
