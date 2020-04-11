using System;

namespace ROS2
{
    public class RuntimeError : Exception
    {
        public RuntimeError()
        {
        }

        public RuntimeError(string message) : base(message)
        {
        }

        public RuntimeError(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class NotInitializedException : Exception
    {
        public NotInitializedException()
        {
        }

        public NotInitializedException(string message) : base(message)
        {
        }

        public NotInitializedException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class InvalidNodeNameException : Exception
    {
        public InvalidNodeNameException()
        {
        }

        public InvalidNodeNameException(string message) : base(message)
        {
        }

        public InvalidNodeNameException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class InvalidNamespaceException : Exception
    {
        public InvalidNamespaceException()
        {
        }

        public InvalidNamespaceException(string message) : base(message)
        {
        }

        public InvalidNamespaceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
