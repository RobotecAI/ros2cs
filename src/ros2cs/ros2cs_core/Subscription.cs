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
  /// <summary> Subscription to a topic with a given type </summary>
  /// <description> Subscriptions are created through INode interface (CreateSubscription) </description>
  public class Subscription<T>: ISubscription<T> where T : Message, new ()
  {
    public rcl_subscription_t Handle { get { return subscriptionHandle; } }
    private rcl_subscription_t subscriptionHandle;

    public string Topic { get { return topic; } }
    private string topic;

    public bool IsDisposed { get { return disposed; } }
    private bool disposed = false;

    private rcl_node_t nodeHandle;
    private readonly Action<T> callback;
    private IntPtr subscriptionOptions;

    public object Mutex { get { return mutex; } }
    private object mutex = new object();

    /// <summary> Tries to get a message from rcl/rmw layers. Calls the callback if successful </summary>
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
        ret = (RCLReturnEnum)NativeRcl.rcl_take(ref subscriptionHandle, message.Handle, IntPtr.Zero, IntPtr.Zero);
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

    /// <summary> Internal constructor for Subscription. Use INode.CreateSubscription to construct </summary>
    /// <see cref="INode.CreateSubscription"/>
    internal Subscription(string subTopic, Node node, Action<T> cb, QualityOfServiceProfile qos = null)
    {
      callback = cb;
      nodeHandle = node.nodeHandle;
      topic = subTopic;
      subscriptionHandle = NativeRcl.rcl_get_zero_initialized_subscription();

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
      {
        qualityOfServiceProfile = new QualityOfServiceProfile();
      }

      subscriptionOptions = NativeRclInterface.rclcs_subscription_create_options(qualityOfServiceProfile.handle);

      T msg = new T();
      MessageInternals msgInternals = msg as MessageInternals;
      IntPtr typeSupportHandle = msgInternals.TypeSupportHandle;
      msg.Dispose();

      Utils.CheckReturnEnum(NativeRcl.rcl_subscription_init(
        ref subscriptionHandle,
        ref node.nodeHandle,
        typeSupportHandle,
        topic,
        subscriptionOptions));
    }

    ~Subscription()
    {
      DestroySubscription();
    }

    public void Dispose()
    {
      DestroySubscription();
    }

    /// <summary> "Destructor" supporting disposable model </summary>
    private void DestroySubscription()
    {
      lock (mutex)
      {
        if (!disposed)
        {
          Utils.CheckReturnEnum(NativeRcl.rcl_subscription_fini(ref subscriptionHandle, ref nodeHandle));
          NativeRclInterface.rclcs_node_dispose_options(subscriptionOptions);
          disposed = true;
          Ros2csLogger.GetInstance().LogInfo("Subscription destroyed");
        }
      }
    }
  }
}
