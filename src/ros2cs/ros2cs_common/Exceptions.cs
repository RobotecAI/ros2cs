using System;

namespace ROS2
{
    public class UnsatisfiedLinkError :Exception {
      public UnsatisfiedLinkError () : base() { }
      public UnsatisfiedLinkError (string message) : base (message) { }
      public UnsatisfiedLinkError (string message, System.Exception inner) : base (message, inner) { }
    }

    public class UnknownPlatformError : Exception {
      public UnknownPlatformError () : base() { }
      public UnknownPlatformError (string message) : base (message) { }
      public UnknownPlatformError (string message, System.Exception inner) : base (message, inner) { }
    }

    public class RuntimeError : Exception
    {
      public RuntimeError() : base() {}
      public RuntimeError(string message) : base(message) {}
      public RuntimeError(string message, Exception inner) : base(message, inner) {}
    }

    public class NotInitializedException : Exception
    {
      public NotInitializedException() : base() {}
      public NotInitializedException(string message) : base(message) {}
      public NotInitializedException(string message, Exception inner) : base(message, inner) {}
    }

    public class InvalidNodeNameException : Exception
    {
      public InvalidNodeNameException() : base() {}
      public InvalidNodeNameException(string message) : base(message) {}
      public InvalidNodeNameException(string message, Exception inner) : base(message, inner) {}
    }

    public class InvalidNamespaceException : Exception
    {
      public InvalidNamespaceException() : base() {}
      public InvalidNamespaceException(string message) : base(message) {}
      public InvalidNamespaceException(string message, Exception inner) : base(message, inner) {}
    }
}
