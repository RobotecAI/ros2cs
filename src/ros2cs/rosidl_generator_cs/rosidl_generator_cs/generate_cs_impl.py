from collections import defaultdict
import os
from rosidl_cmake import generate_files
from rosidl_generator_c import idl_type_to_c
from rosidl_generator_c import value_to_c
from rosidl_generator_c import idl_structure_type_to_c_typename
from rosidl_parser.definition import AbstractGenericString
from rosidl_parser.definition import AbstractNestedType
from rosidl_parser.definition import AbstractSequence
from rosidl_parser.definition import AbstractString
from rosidl_parser.definition import AbstractWString
from rosidl_parser.definition import Array
from rosidl_parser.definition import BasicType
from rosidl_parser.definition import BoundedSequence
from rosidl_parser.definition import FLOATING_POINT_TYPES
from rosidl_parser.definition import NamespacedType
from rosidl_parser.definition import NamedType
from rosidl_parser.definition import UnboundedSequence
import logging
import sys


def generate_cs(generator_arguments_file, typesupport_impls):
    type_support_impl_by_filename = {
        '%s.ep.{0}.c'.format(impl): impl for impl in typesupport_impls
    }

    mapping = {
        'idl.cs.em': '%s.cs',
        'idl.c.em': '%s_s.c'
    }

    #print("Type_support mapping " + str(type_support_impl_by_filename), file=sys.stderr)
    additional_context = {
        'get_field_name' : get_field_name,
        'get_dotnet_type' : get_dotnet_type,
        'constant_value_to_dotnet' : constant_value_to_dotnet,
        'get_c_type' : get_c_type,
        'get_marshal_type' : get_marshal_type,
        'get_marshal_array_type' : get_marshal_array_type
    }

    generate_files(generator_arguments_file, mapping, additional_context)
    for type_support in type_support_impl_by_filename.keys():
        typemapping = { 'idl_typesupport.c.em': type_support }
        generate_files(generator_arguments_file, typemapping)


def escape_string(s):
    s = s.replace('\\', '\\\\')
    s = s.replace('"', '\\"')
    return s


def constant_value_to_dotnet(type_, value):
    assert value is not None

    if isinstance(type_, BasicType) and (type_.typename == 'boolean'):
        return 'true' if value else 'false'

    if isinstance(type_, BasicType) and (type_.typename == 'float'):
        return '%sf' % value

    if isinstance(type_, AbstractGenericString):
        return '"%s"' % escape_string(value)

    return str(value)


def get_builtin_dotnet_type(type_, use_primitives=True):

    if type_ == 'float':
        return 'float' if use_primitives else 'System.Single'

    if type_ == 'double':
        return 'double' if use_primitives else 'System.Double'

    if type_ == 'char':
        return 'char' if use_primitives else 'System.Char'

    if type_ == 'wchar':
        return 'ushort' if use_primitives else 'System.UInt16'

    if type_ == 'boolean':
        return 'bool' if use_primitives else 'System.Boolean'

    if type_ == 'octet':
        return 'byte' if use_primitives else 'System.Byte'

    if type_ == 'int8':
        return 'sbyte' if use_primitives else 'System.Sbyte'

    if type_ == 'uint8':
        return 'byte' if use_primitives else 'System.Byte'

    if type_ == 'int16':
        return 'short' if use_primitives else 'System.Int16'

    if type_ == 'uint16':
        return 'ushort' if use_primitives else 'System.UInt16'

    if type_ == 'int32':
        return 'int' if use_primitives else 'System.Int32'

    if type_ == 'uint32':
        return 'uint' if use_primitives else 'System.UInt32'

    if type_ == 'int64':
        return 'long' if use_primitives else 'System.Int64'

    if type_ == 'uint64':
        return 'ulong' if use_primitives else 'System.UInt64'

    if type_ == 'string':
        return 'System.String'

    assert False, "unknown type '%s'" % type_


def get_c_type(type_):
    if isinstance(type_, AbstractGenericString):
        return 'const char *'
    return idl_type_to_c(type_)


BASIC_IDL_TYPES_TO_MARSHAL = {
    'float': 'R4',
    'double': 'R8',
    'long double': 'R8',
    'char': 'I1',
    'wchar': 'I2',
    'boolean': 'I1',
    'octet': 'U1',
    'uint8': 'U1',
    'int8': 'I1',
    'uint16': 'U2',
    'int16': 'I2',
    'uint32': 'U4',
    'int32': 'I4',
    'uint64': 'U8',
    'int64': 'I8',
    'string': 'LPStr'
}

BASIC_IDL_TYPES_TO_MARSHAL_ARRAY = {
    'float': 'float',
    'double': 'double',
    'long double': 'double',
    'char': 'char',
    'wchar': 'short',
    'boolean': 'byte',
    'octet': 'byte',
    'uint8': 'byte',
    'int8': 'char',
    'uint16': 'short',
    'int16': 'short',
    'uint32': 'int',
    'int32': 'int',
    'uint64': 'long',
    'int64': 'long',
    'string': 'IntPtr'
}


def get_marshal_type(type_):
    if isinstance(type_, BasicType):
        return BASIC_IDL_TYPES_TO_MARSHAL[type_.typename]
    if isinstance(type_, AbstractString):
        return BASIC_IDL_TYPES_TO_MARSHAL['string']
    assert False, "unsupported marshal type '%s'" % type_


def get_marshal_array_type(type_):
    if isinstance(type_, (AbstractSequence, Array)):
        if isinstance(type_.value_type, AbstractString):
            return BASIC_IDL_TYPES_TO_MARSHAL_ARRAY['string']
        return BASIC_IDL_TYPES_TO_MARSHAL_ARRAY[type_.value_type.typename]
    assert False, "unsupported marshal array type '%s'" % type_


def get_dotnet_type(type_, use_primitives=True):
    if isinstance(type_, AbstractGenericString):
        return 'System.String'

    if isinstance(type_, NamespacedType):
        return ".".join(type_.namespaced_name())

    if isinstance(type_, NamedType):
        return type_.name

    if isinstance(type_, (AbstractSequence, Array)):
        return get_dotnet_type(type_.value_type) + "[]"

    return get_builtin_dotnet_type(type_.typename, use_primitives=use_primitives)


def upperfirst(s):
    return s[0].capitalize() + s[1:]


def get_field_name(type_name, field_name, class_name):
    ucased_name = upperfirst(field_name)
    if (ucased_name == type_name) or (ucased_name == class_name):
        return "{0}_".format(ucased_name)
    return ucased_name
