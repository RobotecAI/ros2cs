@#######################################################################
@# Included from rosidl_generator_cs/resource/idl.cs.em
@#
@# Context:
@#  - package_name (string)
@#  - interface_path (Path relative to the directory named after the package)
@#  - message (IdlMessage, with structure containing names, types and members)
@#  - get_dotnet_type, escape_string, get_field_name - helper functions for cs translation of types
@#######################################################################
@{
from rosidl_parser.definition import AbstractNestedType
from rosidl_parser.definition import AbstractGenericString
from rosidl_parser.definition import AbstractString
from rosidl_parser.definition import AbstractWString
from rosidl_parser.definition import Array
from rosidl_parser.definition import AbstractSequence
from rosidl_parser.definition import BasicType
from rosidl_parser.definition import BOOLEAN_TYPE
from rosidl_parser.definition import CHARACTER_TYPES
from rosidl_parser.definition import INTEGER_TYPES
from rosidl_parser.definition import NamespacedType
from rosidl_parser.definition import NamedType
from rosidl_parser.definition import OCTET_TYPE
from rosidl_parser.definition import UNSIGNED_INTEGER_TYPES
from rosidl_generator_c import idl_type_to_c
from rosidl_cmake import convert_camel_case_to_lower_case_underscore
}@
@#>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
@# Handle namespaces
@#<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

@{
message_class = message.structure.namespaced_type.name
message_class_lower = convert_camel_case_to_lower_case_underscore(message_class)
c_full_name = idl_type_to_c(message.structure.namespaced_type)
internals_interface = "MessageInternals"
parent_interface = "Message"
header_type = "std_msgs.msg.Header"
for member in message.structure.members:
  if get_dotnet_type(member.type) == header_type:
    parent_interface = "MessageWithHeader"
}

@[for ns in message.structure.namespaced_type.namespaces]@
namespace @(ns)
{
@[end for]@
// message class
public class @(message_class) : @(internals_interface), @(parent_interface)
{
  private IntPtr _handle;
  private bool disposed;
  private static readonly DllLoadUtils dllLoadUtils;

  // constant declarations
@[for constant in message.constants]@
  public const @(get_dotnet_type(constant.type)) @(constant.name) = @(constant_value_to_dotnet(constant.type, constant.value));
@[end for]@

  // members
@[for member in message.structure.members]@
@[  if isinstance(member.type, (BasicType, AbstractGenericString, NamedType, NamespacedType))]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; set; }
@[    if get_dotnet_type(member.type) == header_type]@

  //Generic interface for all messages with headers
  public void SetHeaderFrame(string frameID)
  {
    @(get_field_name(member.type, member.name, message_class)).Frame_id = frameID;
  }

  public string GetHeaderFrame()
  {
    return @(get_field_name(member.type, member.name, message_class)).Frame_id;
  }

  public void UpdateHeaderTime(int sec, uint nanosec)
  {
    @(get_field_name(member.type, member.name, message_class)).Stamp.Sec = sec;
    @(get_field_name(member.type, member.name, message_class)).Stamp.Nanosec = nanosec;
  }

@[    end if]@
@[  elif isinstance(member.type, (AbstractSequence, Array))]@
@[    if isinstance(member.type.value_type, (BasicType, AbstractGenericString, NamespacedType, NamedType))]@
@# same for now
@[      if isinstance(member.type, AbstractSequence)]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; set; }
@[      elif isinstance(member.type, Array)]@
  public @(get_dotnet_type(member.type)) @(get_field_name(member.type, member.name, message_class)) { get; private set; }
@[      end if]@
@[    end if]@
@[  end if]@
@[end for]@

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate IntPtr NativeGetTypeSupportType();
  private static NativeGetTypeSupportType native_get_typesupport = null;

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate IntPtr NativeCreateNativeMessageType();
  private static NativeCreateNativeMessageType native_create_native_message = null;

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate void NativeDestroyNativeMessageType(IntPtr messageHandle);
  private static NativeDestroyNativeMessageType native_destroy_native_message = null;

@[for member in message.structure.members]@
@[  if isinstance(member.type, AbstractGenericString)]@
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate IntPtr NativeReadField@(get_field_name(member.type, member.name, message_class))Type(IntPtr messageHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate void NativeWriteField@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle, [MarshalAs (UnmanagedType.LPStr)] string value);

@[  elif isinstance(member.type, BasicType)]@
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate @(get_dotnet_type(member.type)) NativeReadField@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate void NativeWriteField@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle, @(get_dotnet_type(member.type)) value);

@[  elif isinstance(member.type, (AbstractSequence, Array))]@
@[    if isinstance(member.type.value_type, BasicType)]@
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate IntPtr NativeReadField@(get_field_name(member.type, member.name, message_class))Type(
    out int array_size,
    IntPtr messageHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate bool NativeWriteField@(get_field_name(member.type, member.name, message_class))Type(
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.@(get_marshal_type(member.type.value_type)), SizeParamIndex = 1)]
      @(get_dotnet_type(member.type)) values,
      int array_size,
      IntPtr messageHandle);
@[    elif isinstance(member.type.value_type, AbstractString)]@
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate IntPtr NativeReadField@(get_field_name(member.type, member.name, message_class))Type(
    int index,
    IntPtr messageHandle);

  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  internal delegate bool NativeWriteField@(get_field_name(member.type, member.name, message_class))Type(
      [MarshalAs (UnmanagedType.LPStr)] string value,
      int index,
      IntPtr messageHandle);

@[    end if]@
@[  end if]@
@[  if isinstance(member.type, (AbstractGenericString, BasicType)) or (isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (BasicType, AbstractString)))]@
  private static NativeReadField@(get_field_name(member.type, member.name, message_class))Type native_read_field_@(member.name) = null;
  private static NativeWriteField@(get_field_name(member.type, member.name, message_class))Type native_write_field_@(member.name) = null;

@[  end if]@
@[  if isinstance(member.type, (NamedType, NamespacedType))]@
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  private delegate IntPtr NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle);
  private static NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type native_get_nested_message_handle_@(member.name) = null;

@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
  private delegate IntPtr NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle, int index);
  private static NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type native_get_nested_message_handle_@(member.name) = null;
@[    end if]@
  private delegate int NativeGetArraySize@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle);
  private static NativeGetArraySize@(get_field_name(member.type, member.name, message_class))Type native_get_array_size_@(member.name) = null;

  private delegate bool NativeInitSequence@(get_field_name(member.type, member.name, message_class))Type(
    IntPtr messageHandle, int size);
  private static NativeInitSequence@(get_field_name(member.type, member.name, message_class))Type native_init_sequence_@(member.name) = null;

@[  end if]@
@[end for]@

  static @(message_class)()
  {
    dllLoadUtils = DllLoadUtilsFactory.GetDllLoadUtils();
    IntPtr nativelibrary = dllLoadUtils.LoadLibrary("@(package_name)_@(message_class_lower)__rosidl_typesupport_c");
    IntPtr native_get_typesupport_ptr = dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_get_type_support");
    @(message_class).native_get_typesupport = (NativeGetTypeSupportType)Marshal.GetDelegateForFunctionPointer(
      native_get_typesupport_ptr, typeof(NativeGetTypeSupportType));

    IntPtr native_create_native_message_ptr = dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_create_native_message");
    @(message_class).native_create_native_message = (NativeCreateNativeMessageType)Marshal.GetDelegateForFunctionPointer(
      native_create_native_message_ptr, typeof(NativeCreateNativeMessageType));

    IntPtr native_destroy_native_message_ptr = dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_destroy_native_message");
    @(message_class).native_destroy_native_message = (NativeDestroyNativeMessageType)Marshal.GetDelegateForFunctionPointer(
      native_destroy_native_message_ptr, typeof(NativeDestroyNativeMessageType));

@[for member in message.structure.members]@
@[  if isinstance(member.type, (BasicType, AbstractGenericString)) or (isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (BasicType, AbstractString)))]@
    IntPtr native_read_field_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_read_field_@(member.name)");
    @(message_class).native_read_field_@(member.name) =
      (NativeReadField@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
      native_read_field_@(member.name)_ptr, typeof(NativeReadField@(get_field_name(member.type, member.name, message_class))Type));

    IntPtr native_write_field_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_write_field_@(member.name)");
    @(message_class).native_write_field_@(member.name) =
      (NativeWriteField@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
      native_write_field_@(member.name)_ptr, typeof(NativeWriteField@(get_field_name(member.type, member.name, message_class))Type));

@[  elif isinstance(member.type, (NamedType, NamespacedType))]@
    IntPtr native_get_nested_message_handle_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_get_nested_message_handle_@(member.name)");
    @(message_class).native_get_nested_message_handle_@(member.name) =
      (NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
      native_get_nested_message_handle_@(member.name)_ptr, typeof(NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type));
@[  end if]@

@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
    IntPtr native_get_nested_message_handle_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_get_nested_message_handle_@(member.name)");
    @(message_class).native_get_nested_message_handle_@(member.name) =
      (NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
    native_get_nested_message_handle_@(member.name)_ptr, typeof(NativeGetNestedHandle@(get_field_name(member.type, member.name, message_class))Type));
@[    end if]@
    IntPtr native_get_array_size_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_get_array_size_@(member.name)");
    @(message_class).native_get_array_size_@(member.name) =
      (NativeGetArraySize@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
    native_get_array_size_@(member.name)_ptr, typeof(NativeGetArraySize@(get_field_name(member.type, member.name, message_class))Type));

    IntPtr native_init_sequence_@(member.name)_ptr =
      dllLoadUtils.GetProcAddress(nativelibrary, "@(c_full_name)_native_init_sequence_@(member.name)");
    @(message_class).native_init_sequence_@(member.name) =
      (NativeInitSequence@(get_field_name(member.type, member.name, message_class))Type)Marshal.GetDelegateForFunctionPointer(
    native_init_sequence_@(member.name)_ptr, typeof(NativeInitSequence@(get_field_name(member.type, member.name, message_class))Type));
@[  end if]@
@[end for]@
  }

  public IntPtr TypeSupportHandle
  {
    get
    {
      return native_get_typesupport();
    }
  }

  // Handle. Create on first use. Can be set for nested classes. TODO -- access...
  public IntPtr Handle
  {
    get
    {
      if (_handle == IntPtr.Zero)
        _handle = native_create_native_message();
      return _handle;
    }
  }

  public @(message_class)()
  {
@[for member in message.structure.members]@
@[  if isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type))();
@[  elif isinstance(member.type, AbstractString)]@
    @(get_field_name(member.type, member.name, message_class)) = "";
@[  elif isinstance(member.type, AbstractSequence)]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[0];
@[  elif isinstance(member.type, Array)]@
    @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[@(member.type.size)];
@[  end if]@
@[end for]@
  }

  public void ReadNativeMessage()
  {
    ReadNativeMessage(Handle);
  }

  public void ReadNativeMessage(IntPtr handle)
  {
    if (handle == IntPtr.Zero)
      throw new System.InvalidOperationException("Invalid handle for reading");
@[for member in message.structure.members]@
@[  if isinstance(member.type, AbstractString)]@
    {
      IntPtr pStr = native_read_field_@(member.name)(handle);
      @(get_field_name(member.type, member.name, message_class)) = Marshal.PtrToStringAnsi(pStr);
    }
@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, BasicType)]@
    { //TODO - (adam) this is a bit clunky. Is there a better way to marshal unsigned and bool types?
      int arraySize = 0;
      IntPtr pArr = native_read_field_@(member.name)(out arraySize, handle);
      @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[arraySize];
      @(get_marshal_array_type(member.type))[] __@(get_field_name(member.type, member.name, message_class)) = new @(get_marshal_array_type(member.type))[arraySize];
      int start = 0;

      Marshal.Copy(pArr, __@(get_field_name(member.type, member.name, message_class)), start, arraySize);
      for (int i = 0; i < arraySize; ++i)
      {
@[    if get_dotnet_type(member.type.value_type) == 'bool']@
        bool _boolean_value = __@(get_field_name(member.type, member.name, message_class))[i] != 0;
        @(get_field_name(member.type, member.name, message_class))[i] = _boolean_value;
@[    elif isinstance(member.type.value_type, AbstractString)]@
        @(get_field_name(member.type, member.name, message_class))[i] = Marshal.PtrToStringAnsi(__@(get_field_name(member.type, member.name, message_class))[i]);
        //Marshal.FreeCoTaskMem(__@(get_field_name(member.type, member.name, message_class))[i]);
@[    else]@
        @(get_field_name(member.type, member.name, message_class))[i] = (@(get_dotnet_type(member.type.value_type)))(__@(get_field_name(member.type, member.name, message_class))[i]);
@[    end if]@
      }
    }
@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
    {
      int __native_array_size = native_get_array_size_@(member.name)(handle);
      @(get_field_name(member.type, member.name, message_class)) = new @(get_dotnet_type(member.type.value_type))[__native_array_size];
      for (int i = 0; i < __native_array_size; ++i)
      {
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
        @(get_field_name(member.type, member.name, message_class))[i] = new @(get_dotnet_type(member.type.value_type))();
        @(get_field_name(member.type, member.name, message_class))[i].ReadNativeMessage(native_get_nested_message_handle_@(member.name)(handle, i));
@[    elif isinstance(member.type.value_type, AbstractString)]@
        @(get_field_name(member.type, member.name, message_class))[i] = Marshal.PtrToStringAnsi(native_read_field_@(member.name)(i, handle));
@[    end if]@
      }
    }
@[  elif isinstance(member.type, BasicType)]@
    @(get_field_name(member.type, member.name, message_class)) = native_read_field_@(member.name)(handle);
@[  elif isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)).ReadNativeMessage(native_get_nested_message_handle_@(member.name)(handle));
@[  end if]@
@[end for]@
  }

  public void WriteNativeMessage()
  {
    if (_handle == IntPtr.Zero)
      _handle = native_create_native_message();
    else
    { //message object reused for subsequent publishing (fields could have changed, and we want to avoid double init on sequences)
      native_destroy_native_message(_handle);
      _handle = native_create_native_message();
      //TODO - do this is handled by checking sequence size internally. Just commenting out these 2 leaks memory
    }
    WriteNativeMessage(Handle);
  }

  //Write from CS to native handle
  public void WriteNativeMessage(IntPtr handle)
  {
    if (handle == IntPtr.Zero)
      throw new System.InvalidOperationException("Invalid handle for writing");
@[for member in message.structure.members]@
@[  if isinstance(member.type, (BasicType, AbstractGenericString))]@
    native_write_field_@(member.name)(handle, @(get_field_name(member.type, member.name, message_class)));
@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, BasicType)]@
    {
      bool success = native_write_field_@(member.name)(@(get_field_name(member.type, member.name, message_class)), @(get_field_name(member.type, member.name, message_class)).Length, handle);
      //TODO - check success
    }
@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
    {
      bool success = native_init_sequence_@(member.name)(handle, @(get_field_name(member.type, member.name, message_class)).Length);
      if (!success)
        throw new System.InvalidOperationException("Error initializing sequence for @(member.name)");
      for (int i = 0; i < @(get_field_name(member.type, member.name, message_class)).Length; ++i)
      {
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
        IntPtr innerHandle = native_get_nested_message_handle_@(member.name)(handle, i);
        @(get_field_name(member.type, member.name, message_class))[i].WriteNativeMessage(innerHandle);
@[    elif isinstance(member.type.value_type, AbstractString)]@
        native_write_field_@(member.name)(@(get_field_name(member.type, member.name, message_class))[i], i, handle);
@[    end if]@
      }
    }
@[  elif isinstance(member.type, (NamedType, NamespacedType))]@
    @(get_field_name(member.type, member.name, message_class)).WriteNativeMessage(native_get_nested_message_handle_@(member.name)(handle));
@[  end if]@
@[end for]@
  }

  public void Dispose()
  {
    if (!disposed)
    {
      if (_handle != IntPtr.Zero)
      {
        native_destroy_native_message(_handle);
        _handle = IntPtr.Zero;
        disposed = true;
      }
    }
  }

  ~@(message_class)()
  {
    Dispose();
  }

};  // class @(message_class)
@#>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
@# close namespaces
@#<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
@[for ns in reversed(message.structure.namespaced_type.namespaces)]@
}  // namespace @(ns)
@[end for]@
