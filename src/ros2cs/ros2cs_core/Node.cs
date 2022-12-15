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
using System.Linq;
using System.Collections.Generic;

namespace ROS2
{
  /// <summary> Represents a managed ros2 (rcl) node </summary>
  /// <see cref="INode"/>
  public class Node: INode
  {
    public string Name { get { return name; } }
    private string name;
    private Ros2csLogger logger = Ros2csLogger.GetInstance();

    internal List<ISubscriptionBase> Subscriptions
    {
      get
      {
        lock (mutex)
        {
          return subscriptions.ToList();
        }
      }
    }
    internal rcl_node_t nodeHandle;
    private IntPtr defaultNodeOptions;
    private HashSet<ISubscriptionBase> subscriptions;
    private HashSet<IPublisherBase> publishers;
    private readonly object mutex = new object();
    private bool disposed = false;

    public bool IsDisposed { get { return disposed; } }

    /// <summary> Node constructor </summary>
    /// <description> Nodes are created through CreateNode method of Ros2cs class </description>
    /// <param name="nodeName"> unique, non-namespaced node name </param>
    /// <param name="context"> (rcl) context for the node. Global context is passed to this method </param>
    internal Node(string nodeName, ref rcl_context_t context)
    {
      name = nodeName;
      string nodeNamespace = "/";
      subscriptions = new HashSet<ISubscriptionBase>();
      publishers = new HashSet<IPublisherBase>();

      nodeHandle = NativeRcl.rcl_get_zero_initialized_node();
      defaultNodeOptions = NativeRclInterface.rclcs_node_create_default_options();
      Utils.CheckReturnEnum(NativeRcl.rcl_node_init(ref nodeHandle, nodeName, nodeNamespace, ref context, defaultNodeOptions));
      logger.LogInfo("Node initialized");
    }

    /// <summary> Finalizer supporting IDisposable model </summary>
    ~Node()
    {
      DestroyNode();
    }

    /// <summary> Release managed and native resources. IDisposable implementation </summary>
    public void Dispose()
    {
      DestroyNode();
    }

    /// <summary> "Destructor" supporting IDisposable model </summary>
    /// <description> Disposes all subscriptions and publishers before finilizing node </description>
    internal void DestroyNode()
    {
      lock (mutex)
      {
        if (!disposed)
        {
          foreach(ISubscriptionBase subscription in subscriptions)
          {
            subscription.Dispose();
          }
          subscriptions.Clear();

          foreach(IPublisherBase publisher in publishers)
          {
            publisher.Dispose();
          }
          publishers.Clear();

          Utils.CheckReturnEnum(NativeRcl.rcl_node_fini(ref nodeHandle));
          NativeRclInterface.rclcs_node_dispose_options(defaultNodeOptions);
          disposed = true;
          logger.LogInfo("Node " + name + " destroyed");
        }
      }
    }

    /// <summary> Create a publisher for this node for a given topic, qos and message type </summary>
    /// <see cref="INode.CreatePublisher"/>
    public Publisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
    {
      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          logger.LogWarning("Cannot create publisher as the class is already disposed or shutdown was called");
          return null;
        }

        Publisher<T> publisher = new Publisher<T>(topic, this, qos);
        publishers.Add(publisher);
        logger.LogInfo("Created Publisher for topic " + topic);
        return publisher;
      }
    }

    /// <summary> Create a subscription for this node for a given topic, callback, qos and message type </summary>
    /// <see cref="INode.CreateSubscription"/>
    public Subscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
    {
      lock (mutex)
      {
        if (disposed || !Ros2cs.Ok())
        {
          logger.LogWarning("Cannot create subscription as the class is already disposed or shutdown was called");
          return null;
        }

        Subscription<T> subscription = new Subscription<T>(topic, this, callback, qos);
        subscriptions.Add(subscription);
        logger.LogInfo("Created subscription for topic " + topic);
        return subscription;
      }
    }

    /// <summary> Remove a publisher </summary>
    /// <see cref="INode.RemovePublisher"/>
    public bool RemovePublisher(IPublisherBase publisher)
    {
      lock (mutex)
      {
        if (publishers.Contains(publisher))
        {
          logger.LogInfo("Removing publisher for topic " + publisher.Topic);
          publisher.Dispose();
          return publishers.Remove(publisher);
        }
        return false;
      }
    }

    /// <summary> Remove a subscription </summary>
    /// <see cref="INode.RemoveSubscription"/>
    public bool RemoveSubscription(ISubscriptionBase subscription)
    {
      lock (mutex)
      {
        if (subscriptions.Contains(subscription))
        {
          logger.LogInfo("Removing subscription for topic " + subscription.Topic);
          subscription.Dispose();
          return subscriptions.Remove(subscription);
        }
        return false;
      }
    }
  }
}
