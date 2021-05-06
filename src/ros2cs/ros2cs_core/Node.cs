using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ROS2
{
    /// <summary>
    /// Represents a managed ROS node
    /// </summary>
    public class Node: INode
    {
        public List<ISubscriptionBase> Subscriptions
        {
          get
          {
            lock (lock_)
            {
              return subscriptions.ToList();
            }
          }
        }

        internal rcl_node_t handle;

        private HashSet<ISubscriptionBase> subscriptions;
        private IntPtr defaultNodeOptions;
        private bool disposed = false;
        private object lock_ = new object();

        private IList<IPublisherBase> publishers;

        //Use Ros2cs.CreateNode to construct
        public Node(string nodeName, Context context, string nodeNamespace = null)
        {
            subscriptions = new HashSet<ISubscriptionBase>();
            publishers = new List<IPublisherBase>();

            if (nodeNamespace == null)
                nodeNamespace = "/";

            if (context.Ok)
            {
                handle = NativeMethods.rcl_get_zero_initialized_node();
                defaultNodeOptions = NativeMethods.rclcs_node_create_default_options();
                Utils.CheckReturnEnum(NativeMethods.rcl_node_init(ref handle, nodeName, nodeNamespace, ref context.handle, defaultNodeOptions));
            }
            else
            {
                throw new NotInitializedException();
            }
        }

        public string Name
        {
            get { return MarshallingHelpers.PtrToString(NativeMethods.rcl_node_get_name(ref handle)); }
        }

        public string Namespace
        {
            get { return MarshallingHelpers.PtrToString(NativeMethods.rcl_node_get_namespace(ref handle)); }
        }

        ~Node()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                lock (lock_)
                {
                    subscriptions.Clear();

                    foreach(IPublisherBase publisher in publishers)
                    {
                        publisher.Dispose();
                    }
                    publishers.Clear();
                    DestroyNode();

                    disposed = true;
                }
            }
        }

        public void DestroyNode()
        {
            Utils.CheckReturnEnum(NativeMethods.rcl_node_fini(ref handle));
            NativeMethods.rclcs_node_dispose_options(defaultNodeOptions);
        }

        public Publisher<T> CreatePublisher<T>(string topic, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            Publisher<T> publisher = new Publisher<T>(topic, this, qos);
            publishers.Add(publisher);
            return publisher;
        }

        public Subscription<T> CreateSubscription<T>(string topic, Action<T> callback, QualityOfServiceProfile qos = null) where T : Message, new()
        {
            Subscription<T> subscription = new Subscription<T>(topic, this, callback, qos);
            lock (lock_)
            {
                subscriptions.Add(subscription);
            }
            return subscription;
        }

        public bool RemoveSubscription(ISubscriptionBase subscription)
        {
            lock (lock_)
            {
                if (subscriptions != null && subscriptions.Contains(subscription))
                {
                    return subscriptions.Remove(subscription);
                }
                return false;
            }
        }
    }
}
