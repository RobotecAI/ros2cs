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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

    /// <inheritdoc/>
    public IReadOnlyDictionary<long, Task<O>> PendingRequests {get; private set;}

    /// <inheritdoc/>
    IReadOnlyDictionary<long, Task> IClientBase.PendingRequests {get { return (IReadOnlyDictionary<long, Task>)this.PendingRequests; }}

    private string topic;

    /// <inheritdoc/>
    public object Mutex { get { return mutex; } }

    private object mutex = new object();

    /// <summary>
    /// Mapping from request id without Response to <see cref="TaskCompletionSource"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="TaskCompletionSource.Task"/> is stored separately to allow
    /// <see cref="Cancel"/> to work even if the source returns multiple tasks.
    /// </remarks>
    private Dictionary<long, (TaskCompletionSource<O>, Task<O>)> Requests;

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

      Requests = new Dictionary<long, (TaskCompletionSource<O>, Task<O>)>();
      PendingRequests = new PendingTasksView(Requests);

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
              bool success = source.Item1.TrySetException(new ObjectDisposedException("client has been disposed"));
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
      rcl_rmw_request_id_t request_header = default(rcl_rmw_request_id_t);
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
      bool exists = false;
      (TaskCompletionSource<O>, Task<O>) source = default((TaskCompletionSource<O>, Task<O>));
      lock (Requests)
      {
        if (Requests.TryGetValue(sequence_number, out source))
        {
          exists = true;
          Requests.Remove(sequence_number);
        }
      }
      if (exists)
      {
        msg.ReadNativeMessage();
        source.Item1.SetResult((O)msg);
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
      long sequence_number = default(long);
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
    /// <returns>The associated task.</returns>
    private Task<O> RegisterSource(TaskCompletionSource<O> source, long sequence_number)
    {
      Task<O> task = source.Task;
      lock (Requests)
      {
        Requests.Add(sequence_number, (source, task));
      }
      return task;
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
          return RegisterSource(source, sequence_number);
      }
    }

    /// <inheritdoc/>
    public bool Cancel(Task task)
    {
      var pair = default(KeyValuePair<long, (TaskCompletionSource<O>, Task<O>)>);
      try
      {
        lock(this.Requests)
        {
          pair = this.Requests.First(entry => entry.Value.Item2 == task);
          // has to be true
          this.Requests.Remove(pair.Key);
        }
      }
      catch (InvalidOperationException)
      {
        return false;
      }
      pair.Value.Item1.SetCanceled();
      return true;
    }

    /// <summary>
    /// Wrapper to avoid exposing <see cref="TaskCompletionSource"/> to users.
    /// </summary>
    /// <remarks>
    /// The locking used is required because the user may access the view while <see cref="Client.TakeMessage"/> is running.
    /// </remarks>
    private class PendingTasksView : IReadOnlyDictionary<long, Task<O>>, IReadOnlyDictionary<long, Task>
    {
      public Task<O> this[long key]
      {
        get
        {
          lock (this.Requests)
          {
            return this.Requests[key].Item2;
          }
        }
      }

      Task IReadOnlyDictionary<long, Task>.this[long key]
      {
        get { return this[key]; }
      }

      public IEnumerable<long> Keys
      {
        get
        {
          lock (this.Requests)
          {
            return this.Requests.Keys.ToArray();
          }
        }
      }

      public IEnumerable<Task<O>> Values
      {
        get
        {
          lock (this.Requests)
          {
            return this.Requests.Values.Select(value => value.Item2).ToArray();
          }
        }
      }

      IEnumerable<Task> IReadOnlyDictionary<long, Task>.Values
      {
        get { return this.Values; }
      }

      public int Count
      {
        get
        {
          lock (this.Requests)
          {
            return this.Requests.Count;
          }
        }
      }

      private readonly IReadOnlyDictionary<long, (TaskCompletionSource<O>, Task<O>)> Requests;

      public PendingTasksView(IReadOnlyDictionary<long, (TaskCompletionSource<O>, Task<O>)> requests)
      {
        this.Requests = requests;
      }

      public bool ContainsKey(long key)
      {
        lock (this.Requests)
        {
          return this.Requests.ContainsKey(key);
        }
      }

      public bool TryGetValue(long key, out Task<O> value)
      {
        bool success = false;
        (TaskCompletionSource<O>, Task<O>) source = default((TaskCompletionSource<O>, Task<O>));
        lock (this.Requests)
        {
          success = this.Requests.TryGetValue(key, out source);
        }
        value = source.Item2;
        return success;
      }

      bool IReadOnlyDictionary<long, Task>.TryGetValue(long key, out Task value)
      {
        bool success = this.TryGetValue(key, out var task);
        value = task;
        return success;
      }

      public IEnumerator<KeyValuePair<long, Task<O>>> GetEnumerator()
      {
        lock (this.Requests)
        {
          return this.Requests
            .Select(pair => new KeyValuePair<long, Task<O>>(pair.Key, pair.Value.Item2))
            .ToArray()
            .AsEnumerable()
            .GetEnumerator();
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.GetEnumerator();
      }

      IEnumerator<KeyValuePair<long, Task>> IEnumerable<KeyValuePair<long, Task>>.GetEnumerator()
      {
        lock (this.Requests)
        {
          return this.Requests
            .Select(pair => new KeyValuePair<long, Task>(pair.Key, pair.Value.Item2))
            .ToArray()
            .AsEnumerable()
            .GetEnumerator();
        }
      }
    }
  }
}
