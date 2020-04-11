using NUnit.Framework;
using System;

namespace ROS2.Test
{
    [TestFixture]
    public class InitShutdownTest
    {
        [Test]
        public void Init()
        {
            Context context = new Context();
            Ros2cs.Init(context);
            try
            {
                Ros2cs.Shutdown(context);
            }
            catch (RuntimeError)
            {
            }
        }

        [Test]
        public void InitShutdown()
        {
           Context context = new Context();
            Ros2cs.Init(context);
            Ros2cs.Shutdown(context);
        }

        [Test]
        public void InitShutdownSequence()
        {
            // local
            Context context = new Context();
            Ros2cs.Init(context);
            Ros2cs.Shutdown(context);
            context = new Context();
            Ros2cs.Init(context);
            Ros2cs.Shutdown(context);
        }

        [Test]
        public void DoubleInit()
        {
            Context context = new Context();
            Ros2cs.Init(context);
            Assert.That(() => { Ros2cs.Init(context); }, Throws.TypeOf<RuntimeError>());
            Ros2cs.Shutdown(context);
        }

        [Test]
        public void DoubleShutdown()
        {
            Context context = new Context();
            Ros2cs.Init(context);
            Ros2cs.Shutdown(context);
            Assert.That(() => { Ros2cs.Shutdown(context); }, Throws.TypeOf<RuntimeError>());
        }

        [Test]
        public void CreateNodeWithoutInit()
        {
            Context context = new Context();
            Assert.That(() => { Ros2cs.CreateNode("foo", context); }, Throws.TypeOf<NotInitializedException>());
        }


    }

}
