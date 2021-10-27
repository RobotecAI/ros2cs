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
using ROS2.Internal;

namespace ROS2
{
  /// <summary> Publisher of a topic with a given type </summary>
  /// <description> Publishers are created through INode.CreatePublisher </description>
  public class Publisher<T>: IPublisher<T> where T : Message, new ()
  {
    public string Topic { get { return topic; } }
    private string topic;

    private Ros2csLogger logger = Ros2csLogger.GetInstance();
    rcl_publisher_t publisherHandle;
    IntPtr publisherOptions = IntPtr.Zero;
    rcl_node_t nodeHandle;
    private bool disposed = false;

    public bool IsDisposed { get { return disposed; } }

    /// <summary> Internal constructor for Publsher. Use INode.CreatePublisher to construct </summary>
    /// <see cref="INode.CreatePublisher"/>
    public Publisher(string pubTopic, Node node, QualityOfServiceProfile qos = null)
    {
      topic = pubTopic;
      nodeHandle = node.nodeHandle;

      QualityOfServiceProfile qualityOfServiceProfile = qos;
      if (qualityOfServiceProfile == null)
        qualityOfServiceProfile = new QualityOfServiceProfile();

      publisherOptions = NativeRclInterface.rclcs_publisher_create_options(qualityOfServiceProfile.handle);

      IntPtr typeSupportHandle = MessageTypeSupportHelper.GetTypeSupportHandle<T>();

      publisherHandle = NativeRcl.rcl_get_zero_initialized_publisher();
      Utils.CheckReturnEnum(NativeRcl.rcl_publisher_init(
                              ref publisherHandle,
                              ref nodeHandle,
                              typeSupportHandle,
                              topic,
                              publisherOptions));
    }

    ~Publisher()
    {
      Dispose();
    }

    public void Dispose()
    {
      DestroyPublisher();
    }

    /// <summary> "Destructor" supporting disposable model </summary>
    private void DestroyPublisher()
    {
      if (!disposed)
      {
        Utils.CheckReturnEnum(NativeRcl.rcl_publisher_fini(ref publisherHandle, ref nodeHandle));
        NativeRclInterface.rclcs_publisher_dispose_options(publisherOptions);
        logger.LogInfo("Publisher destroyed");
        disposed = true;
      }
    }

    /// <summary> Publish a message </summary>
    /// <see cref="IPublisher.Publish"/>
    public void Publish(T msg)
    {
      if (!Ros2cs.Ok() || disposed)
      {
        logger.LogWarning("Cannot publish as the class is already disposed or shutdown was called");
        return;
      }
      MessageInternals msgInternals = msg as MessageInternals;
      msgInternals.WriteNativeMessage();
      Utils.CheckReturnEnum(NativeRcl.rcl_publish(ref publisherHandle, msgInternals.Handle, IntPtr.Zero));
    }
  }
}
