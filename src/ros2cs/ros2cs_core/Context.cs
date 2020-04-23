using System;

namespace ROS2
{
	/// <summary>
	/// Represents a managed ROS context
	/// </summary>
	public class Context: IDisposable
	{
        internal rcl_context_t handle;
        private rcl_allocator_t allocator;
				private bool IsInitialized;

        public Context()
        {
						IsInitialized = false;
        }

        public bool Ok
        {
            get { return IsInitialized && NativeMethods.rcl_context_is_valid(ref handle); }
        }

				//To keep clarity with rcl naming
				internal void Shutdown()
				{
						Dispose(true);
				}

				internal void Init()
				{
						allocator = NativeMethods.rcutils_get_default_allocator();
						handle = NativeMethods.rcl_get_zero_initialized_context();
						Utils.CheckReturnEnum(NativeMethods.rclcs_init(ref handle, allocator));
						IsInitialized = true;
				}

        public void Dispose()
        {
            Shutdown();
        }

        private void Dispose(bool disposing)
        {
            if (IsInitialized)
            {
                Utils.CheckReturnEnum(NativeMethods.rcl_shutdown(ref handle));
                NativeMethods.rcl_context_fini(ref handle);
								IsInitialized = false;
            }
        }

        ~Context()
        {
            Dispose(false);
        }
    }
}
