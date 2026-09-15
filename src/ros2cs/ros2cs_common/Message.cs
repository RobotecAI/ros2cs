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

namespace ROS2
{
  /// <summary>Encoding helpers for ROS string fields (UTF-8 on every platform).</summary>
  public static class Ros2csText
  {
    // LPUTF8Str is supported by the runtime but absent from the netstandard2.0 enum.
    public const System.Runtime.InteropServices.UnmanagedType Utf8String =
      (System.Runtime.InteropServices.UnmanagedType)48;

    public static string PtrToStringUtf8(IntPtr pointer)
    {
      if (pointer == IntPtr.Zero) return null;
      int length = 0;
      while (System.Runtime.InteropServices.Marshal.ReadByte(pointer, length) != 0) length++;
      byte[] bytes = new byte[length];
      System.Runtime.InteropServices.Marshal.Copy(pointer, bytes, 0, length);
      return System.Text.Encoding.UTF8.GetString(bytes);
    }
  }

  /// <summary> Basic Message interface, only exposing Dispose method <summary>
  public interface Message : IExtendedDisposable
  {
  }

  /// <summary> Convenience interface to manipulate headers <summary>
  public interface MessageWithHeader : Message
  {
    void SetHeaderFrame(string frameID);
    string GetHeaderFrame();
    void UpdateHeaderTime(int sec, uint nanosec);
  }
}  // namespace ROS2
