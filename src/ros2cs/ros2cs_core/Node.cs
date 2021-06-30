using System;
using System.Linq;
using System.Collections.Generic;

namespace ROS2
{
    /// <summary>
    /// Represents a managed ROS node
    /// </summary>
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
        private bool disposed = false;
        private readonly object mutex = new object();

        internal Node(string nodeName, ref rcl_context_t context)
        {
          name = nodeName;
          string nodeNamespace = "/";
          subscriptions = new HashSet<ISubscriptionBase>();
          publishers = new HashSet<IPublisherBase>();

          nodeHandle = NativeMethods.rcl_get_zero_initialized_node();
          defaultNodeOptions = NativeMethods.rclcs_node_create_default_options();
          Utils.CheckReturnEnum(NativeMethods.rcl_node_init(ref nodeHandle, nodeName, nodeNamespace, ref context, defaultNodeOptions));
          logger.LogInfo("Node initialized");
        }

        ~Node()
        {
          DestroyNode();
        }

        public void Dispose()
        {
          DestroyNode();
        }

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

              Utils.CheckReturnEnum(NativeMethods.rcl_node_fini(ref nodeHandle));
              NativeMethods.rclcs_node_dispose_options(defaultNodeOptions);
              disposed = true;
              logger.LogInfo("Node " + name + " destroyed");
            }
          }
        }

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

        public bool RemoveSubscription(ISubscriptionBase subscription)
        {
          lock (mutex)
          {
            if (subscriptions.Contains(subscription))
            {
              logger.LogInfo("Removing subscription for topic " + subscription.Topic);
              return subscriptions.Remove(subscription);
            }
            return false;
          }
        }

        public bool RemovePublisher(IPublisherBase publisher)
        {
          lock (mutex)
          {
            if (publishers.Contains(publisher))
            {
              logger.LogInfo("Removing publisher for topic " + publisher.Topic);
              return publishers.Remove(publisher);
            }
            return false;
          }
        }
    }
}
