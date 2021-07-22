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

namespace ROS2
{
  /// <summary> Primary ros2 C# static class </summary>
  /// <description> This class interfaces with rcl library to handle initalization, shutdown,
  /// creation and removal of nodes as well as spinning (no executors are implemented).
  /// Note that the interface is through rcl and not rclcpp, the primary reason is that marshalling
  /// into generic interface api is not feasible, especially when we don't know all possible instantiations
  /// (as it is the case with custom generated messages).
  /// </description>
  public static class Ros2cs
  {
    private static readonly Destructor destructor = new Destructor();
    private static readonly object mutex = new object();
    private static bool initialized = false;  // for most part equivalent to rcl::ok()
    private static rcl_context_t global_context;  // a simplification, we only use global default context
    private static rcl_allocator_t default_allocator;
    private static List<INode> nodes = new List<INode>(); // kept to shutdown everything in order

    /// <summary> Globally initialize ros2 (rcl) </summary>
    /// <description> Note that only a single context is used. </description>
    /// <remarks> If needed, support for multiple contexts can be added
    /// in a rather straightforward way throughout api. </remarks>
    public static void Init()
    {
      lock (mutex)
      {
        if (initialized)
        {
          return;
        }

        default_allocator = NativeRcl.rcutils_get_default_allocator();
        global_context = NativeRcl.rcl_get_zero_initialized_context();
        Utils.CheckReturnEnum(NativeRclInterface.rclcs_init(ref global_context, default_allocator));
        initialized = true;
      }
    }

    /// <summary> Globally shutdown ros2 (rcl) </summary>
    /// <description> Can be called multiple times with no effects after the first one.
    /// Shutdowns ros2 and disposes all the nodes. Ok() function will return false after Shutdown is called.
    /// </description>
    public static void Shutdown()
    {
      lock (mutex)
      {
        if (!initialized)
        {
          return;
        }
        initialized = false;

        Ros2csLogger.GetInstance().LogInfo("Ros2cs shutdown");
        Utils.CheckReturnEnum(NativeRcl.rcl_shutdown(ref global_context));

        foreach (var node in nodes)
        {
          node.Dispose();
        }
        nodes.Clear();
      }
    }

    /// <summary> Whether ros2 C# is initialized </summary>
    /// <description>
    /// Only when this function returns true a node can be created and spinning works
    /// </description>
    public static bool Ok()
    {
      return initialized && NativeRcl.rcl_context_is_valid(ref global_context);
    }

    /// <summary> Helper class to handle Ros2cs finalization </summary>
    /// <description> Could be understood as Ros2cs destructor. Can be called from GC if Shutdown
    /// was not called explicitly. Also, handles context finalization. </description>
    private sealed class Destructor
    {
      ~Destructor()
      {
        Ros2csLogger.GetInstance().LogInfo("Ros2cs destructor called");
        Ros2cs.Shutdown();
        NativeRcl.rcl_context_fini(ref global_context);
      }
    }

    /// <summary> Create a ros2 (rcl) node </summary>
    /// <description> Creates a node in the global context and adds it to an internal collection.
    /// Checks for name uniqueness. Throws if name is not unique or Ok() is not true. </description>
    /// <remarks> Note that node options are not exposed. Default node options are used.
    /// This can be extended by exposing desired configurations and adding a library call to set
    /// them in the native code. </remarks>
    /// <param name="nodeName"> A valid node name, which will be first checked for uniqueness,
    /// then validated inside rcl according to naming rules (will throw exception if invalid). </param>
    /// <returns> INode interface, which can be used to create subs and pubs </returns>
    public static INode CreateNode(string nodeName)
    {
      lock (mutex)
      {
        if (!Ok())
        {
          Ros2csLogger.GetInstance().LogError("Ros2cs is not initialized, cannot create node");
          throw new NotInitializedException();
        }

        foreach (var node in nodes)
        {
          if (node.Name == nodeName)
          {
            throw new InvalidOperationException("Node with name " + nodeName + " already exists, cannot create");
          }
        }

        var new_node = new Node(nodeName, ref global_context);
        nodes.Add(new_node);
        return new_node;
      }
    }

    /// <summary> Remove and dispose ros2 (rcl) node </summary>
    /// <remarks> You can call Shutdown to dispose all the nodes, this is only needed when
    /// node needs to be removed while others are still meant to be running </remarks>
    /// <param name="node"> a node to remove as returned by previous CreateNode call </param>
    /// <returns> Whether the node was in the internal collection, which should always be true
    /// unless this is called more than once for a node (which is ok). Return value can be
    /// safely ignored <returns>
    public static bool RemoveNode(INode node)
    {
      lock (mutex)
      {
        if (!initialized)
        {
          return false; // removal is handled with shutdown already
        }
        node.Dispose();
        return nodes.Remove(node);
      }
    }

    /// <summary> Spin on a single node </summary>
    /// <description> Spin should be called in a dedicate spinning thread in your
    /// application layer since it runs in a blocking infinite loop. Will return when some work is
    /// executed (a callback for each subscription that received a message) or after a timeout.
    /// Note that you don't need to spin if you are only publishing (like in ros2) </description>
    /// <remarks> Only subscriptions are executed currently, no timers or other executables </remarks>
    /// <param name="node"> A node to spin on </param>
    /// <param name="timoutSec"> Maximum time to wait for execution item (e. g. subscription) </param>
    public static void Spin(INode node, double timeoutSec = 0.1)
    {
      var nodes = new List<INode>{ node };
      Spin(nodes, timeoutSec);
    }

    /// <summary> Spin overload for multiple nodes </summary>
    /// <remarks> This overload saves on implicit List creation </remarks>
    /// <see cref="Spin(INode,double)"/>
    public static void Spin(List<INode> nodes, double timeoutSec = 0.1)
    {
      while (initialized)
      {
        SpinOnce(nodes, timeoutSec);
      }
    }

    /// <summary> Spin only once </summary>
    /// <description> This overload is meant for when the while loop is better to
    /// handle in the application layer  </description>
    /// <see cref="Spin(INode,double)"/>
    public static void SpinOnce(INode node, double timeoutSec = 0.1)
    {
      var nodes = new List<INode>{ node };
      SpinOnce(nodes, timeoutSec);
    }

    /// <summary> SpinOnce overload for multiple nodes </summary>
    /// <remarks> This overload saves on implicit List creation </remarks>
    /// <see cref="SpinOnce(INode,double)"/>
    private static bool warned_once = false;
    public static void SpinOnce(List<INode> nodes, double timeoutSec = 0.1)
    {
      lock (mutex)
      {  // Figure out how to minimize this lock
        if (!initialized)
        {
          return;
        }

        if (timeoutSec < 0.0001d)
        {
          timeoutSec = 0.0001d;

          if (!warned_once)
          {
            Ros2csLogger.GetInstance().LogWarning("Spin timeout too low. Changed to a minimum value of " + timeoutSec.ToString());
            warned_once = true;
          }
        }

        // TODO - This can be optimized so that we cache the list and invalidate only with changes
        var allSubscriptions = new List<ISubscriptionBase>();
        foreach (INode node_interface in nodes)
        {
          Node node = node_interface as Node;
          if (node == null)
            continue; //Rare situation in which we are disposing

          foreach(ISubscriptionBase subscription in node.Subscriptions)
          {
            if (subscription == null)
              continue; //Rare situation in which we are disposing

            allSubscriptions.Add(subscription);
          }
        }

        // TODO - investigate performance impact
        WaitSet.Wait(global_context, allSubscriptions, timeoutSec);

        // Sequential processing
        foreach (var subscription in allSubscriptions)
        {
          subscription.TakeMessage();
        }
      }
    }
  }
}
