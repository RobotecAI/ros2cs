// Copyright 2019-2021 Robotec.ai
// Copyright 2019 Dyno Robotics (by Samuel Lindgren samuel@dynorobotics.se)
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
    /// <summary>
    /// Exception thrown when trying to wait on an empty wait set.
    /// </summary>
    public class WaitSetEmptyException : InvalidOperationException
    {
      public WaitSetEmptyException() : base()
      { }

      /// <inheritdoc />
      public WaitSetEmptyException(string message) : base(message)
      { }

      /// <inheritdoc />
      public WaitSetEmptyException(string message, Exception innerException) : base(message, innerException)
      { }
    }
}
