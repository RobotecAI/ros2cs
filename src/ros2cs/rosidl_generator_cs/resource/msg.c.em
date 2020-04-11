// generated from rosidl_generator_cs/resource/msg.c.em
// generated code does not contain a copyright notice

@#######################################################################
@# EmPy template for generating <msg>_s.c files
@#
@# Context:
@#  - package_name (string)
@#  - interface_path (Path relative to the directory named after the package)
@#  - message (IdlMessage, with structure containing names, types and members)
@#  - idl_type_to_c, value_to_c, idl_structure_type_to_c_typename - converting names, types and values from idl to c
@#######################################################################

@{
from rosidl_generator_c import idl_type_to_c
from rosidl_generator_c import idl_structure_type_to_c_typename
from rosidl_cmake import convert_camel_case_to_lower_case_underscore
from rosidl_parser.definition import AbstractString
from rosidl_parser.definition import AbstractGenericString
from rosidl_parser.definition import BasicType
from rosidl_parser.definition import NamespacedType
from rosidl_parser.definition import NamedType
from rosidl_parser.definition import AbstractSequence
from rosidl_parser.definition import Array
}@

#include <stdlib.h>
#include <stdio.h>
#include <assert.h>
#include <stdint.h>
#include <string.h>

@{
msg_typename = idl_structure_type_to_c_typename(message.structure.namespaced_type)
include_path = "/".join(include_parts)
have_not_included_string = True
have_not_included_arrays = True
}@
#include <@(include_path)__struct.h>
#include <@(include_path)__functions.h>
@[for member in message.structure.members]@
@[  if isinstance(member.type, AbstractGenericString) or (isinstance(member.type, AbstractSequence) and isinstance(member.type.value_type, AbstractString)) and have_not_included_string]@
@{have_not_included_string = False}@
#include <rosidl_generator_c/string.h>
#include <rosidl_generator_c/string_functions.h>

@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, BasicType) and have_not_included_arrays]@
@{have_not_included_arrays = False}@
#include <rosidl_generator_c/primitives_sequence.h>
#include <rosidl_generator_c/primitives_sequence_functions.h>
@[  elif isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType))]@
#include <@("/".join(member.type.value_type.namespaces))/@(convert_camel_case_to_lower_case_underscore(member.type.value_type.name))__functions.h>
@[  end if]@
@[end for]@

@[for member in message.structure.members]@
@[  if isinstance(member.type, BasicType) or isinstance(member.type, AbstractGenericString)]@
ROSIDL_GENERATOR_C_EXPORT
@(get_c_type(member.type)) @(msg_typename)_native_read_field_@(member.name)(void *message_handle)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if  isinstance(member.type, AbstractGenericString)]@
  return ros_message->@(member.name).data;
@[    else]@
  return ros_message->@(member.name);
@[    end if]@
}
@[  end if]@
@[end for]@

@[for member in message.structure.members]@
@[  if isinstance(member.type, BasicType) or isinstance(member.type, AbstractGenericString)]@
ROSIDL_GENERATOR_C_EXPORT
void @(msg_typename)_native_write_field_@(member.name)(void *message_handle, @(get_c_type(member.type)) value)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if  isinstance(member.type, AbstractGenericString)]@
  rosidl_generator_c__String__assign(
    &ros_message->@(member.name), value);
@[    else]@
  ros_message->@(member.name) = value;
@[    end if]@
}
@[  end if]@
@[end for]@


@[for member in message.structure.members]@
@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, BasicType)]@
ROSIDL_GENERATOR_C_EXPORT
bool @(msg_typename)_native_write_field_@(member.name)(@(get_c_type(member.type.value_type)) *value, int size, void *message_handle)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if isinstance(member.type, Array)]@
  if (size != @(member.type.size))
    return false;
  @(get_c_type(member.type.value_type)) *dest = ros_message->@(member.name);
@[    elif isinstance(member.type, AbstractSequence)]@
  size_t previous_sequence_size = ros_message->@(member.name).size;
  if (previous_sequence_size != (size_t)size && previous_sequence_size != 0)
    rosidl_generator_c__@(member.type.value_type.typename)__Sequence__fini(&ros_message->@(member.name));
  if (!rosidl_generator_c__@(member.type.value_type.typename)__Sequence__init(&ros_message->@(member.name), size))
    return false;
  @(get_c_type(member.type.value_type)) *dest = ros_message->@(member.name).data;
@[    end if]@
  memcpy(dest, value, sizeof(@(get_c_type(member.type.value_type)))*size);
  return true;
}
@[  end if]@
@[end for]@


@[for member in message.structure.members]@
@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, BasicType)]@
ROSIDL_GENERATOR_C_EXPORT
@(get_c_type(member.type.value_type)) *@(msg_typename)_native_read_field_@(member.name)(void *message_handle, int *size)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if isinstance(member.type, Array)]@
  *size = @(member.type.size);
  return ros_message->@(member.name);
@[    elif isinstance(member.type, AbstractSequence)]@
  *size = ros_message->@(member.name).size;
  return ros_message->@(member.name).data;
@[    end if]@
}
@[  end if]@
@[end for]@


@[for member in message.structure.members]@
@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, AbstractString)]@
ROSIDL_GENERATOR_C_EXPORT
bool @(msg_typename)_native_write_field_@(member.name)(char *value, int index, void *message_handle)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if isinstance(member.type, Array)]@
  if (index >= @(member.type.size))
      return false;
  rosidl_generator_c__String__assign(&ros_message->@(member.name)[index], value);
@[    elif isinstance(member.type, AbstractSequence)]@
  rosidl_generator_c__String__assign(&ros_message->@(member.name).data[index], value);
@[    end if]@
  return true;
}
@[  end if]@
@[end for]@

@[for member in message.structure.members]@
@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, AbstractString)]@
ROSIDL_GENERATOR_C_EXPORT
char *@(msg_typename)_native_read_field_@(member.name)(int index, void *message_handle)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if isinstance(member.type, Array)]@
  return ros_message->@(member.name)[index].data;
@[    elif isinstance(member.type, AbstractSequence)]@
  return ros_message->@(member.name).data[index].data;
@[    end if]@
}
@[  end if]@
@[end for]@

@[for member in message.structure.members]@
@[if isinstance(member.type, (NamedType, NamespacedType))]
@{
n_type = get_c_type(member.type.name) if isinstance(member.type, NamedType) else idl_structure_type_to_c_typename(member.type)
}
ROSIDL_GENERATOR_C_EXPORT
void * @(msg_typename)_native_get_nested_message_handle_@(member.name)(void *message_handle)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
  @(n_type) *nested_message = &(ros_message->@(member.name));
  return (void *)nested_message;
}
@[end if]@
@[end for]@

@[for member in message.structure.members]@
@[  if isinstance(member.type, (AbstractSequence, Array)) and isinstance(member.type.value_type, (NamedType, NamespacedType, AbstractString))]@
@{
n_type = get_c_type(member.type.value_type.name) if isinstance(member.type.value_type, (NamedType)) else idl_structure_type_to_c_typename(member.type.value_type) if isinstance(member.type.value_type, NamespacedType) else idl_type_to_c(member.type.value_type)
}
@[    if isinstance(member.type.value_type, (NamedType, NamespacedType))]@
ROSIDL_GENERATOR_C_EXPORT
void * @(msg_typename)_native_get_nested_message_handle_@(member.name)(void *message_handle, int index)
{
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
@[    if isinstance(member.type, Array)]@
  @(n_type) *nested_message = &(ros_message->@(member.name)[index]); //TODO - assert size!
@[    elif isinstance(member.type, AbstractSequence)]@
  @(n_type) *nested_message = &(ros_message->@(member.name).data[index]); //TODO - assert size!
@[    end if]@
  return (void *)nested_message;
}
@[    end if]@

ROSIDL_GENERATOR_C_EXPORT
int @(msg_typename)_native_get_array_size_@(member.name)(void *message_handle)
{
@[    if isinstance(member.type, Array)]@
  (void)message_handle;  //TODO - message handle not used
  return @(member.type.size);
@[    elif isinstance(member.type, AbstractSequence)]@
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
  return ros_message->@(member.name).size;
@[    end if]@
}

ROSIDL_GENERATOR_C_EXPORT
bool @(msg_typename)_native_init_sequence_@(member.name)(void *message_handle, int size)
{
@[    if isinstance(member.type, Array)]@
  //TODO - remove this function call for Arrays altogether
  (void)message_handle; //TODO - message handle not used
  if (size != @(member.type.size))
    return false;
  return true;
@[    else]@
  @(msg_typename) *ros_message = (@(msg_typename) *)message_handle;
  size_t previous_sequence_size = ros_message->@(member.name).size;
  if (previous_sequence_size != (size_t)size && previous_sequence_size != 0)
    @(n_type)__Sequence__fini(&ros_message->@(member.name)); //Supports same message reuse
  if (!@(n_type)__Sequence__init(&ros_message->@(member.name), size))
    return false;
  return true;
@[    end if]@
}
@[  end if]@
@[end for]@
