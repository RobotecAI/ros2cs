using System;

namespace ROS2
{
	/// <summary>
	/// Represents a managed ROS context
	/// </summary>
	public class Context: IDisposable
	{
        //TODO: Init context when created?
        internal rcl_context_t handle;
        private rcl_allocator_t allocator;

        public bool isInit;
        private bool disposed;

        public Context()
        {
            allocator = NativeMethods.rcutils_get_default_allocator();
            handle = NativeMethods.rcl_get_zero_initialized_context();
        }

        public void Init()
        {
            Utils.CheckReturnEnum(NativeMethods.rclcs_init(ref handle, allocator));
            isInit = true;
        }

        public void Shutdown()
        {
            Utils.CheckReturnEnum(NativeMethods.rcl_shutdown(ref handle));
            isInit = false;
        }

        public bool Ok
        {
            get { return NativeMethods.rcl_context_is_valid(ref handle); }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if(isInit)
                {
                    Shutdown();
                }
                NativeMethods.rcl_context_fini(ref handle);
                disposed = true;
            }
        }

        ~Context()
        {
            Dispose(false);
        }

    }

}
