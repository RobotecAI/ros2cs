# Copyright 2019-2021 Robotec.ai
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from rosidl_cmake import generate_files
from rosidl_generator_c import idl_type_to_c
from rosidl_parser.definition import AbstractGenericString
from rosidl_parser.definition import AbstractSequence
from rosidl_parser.definition import AbstractString
from rosidl_parser.definition import Array
from rosidl_parser.definition import BasicType
from rosidl_parser.definition import NamespacedType
from rosidl_parser.definition import NamedType


def generate_cs(generator_arguments_file, typesupport_impls, cs_build_tool):
    type_support_impl_by_filename = {
        '%s.ep.{0}.c'.format(impl): impl for impl in typesupport_impls
    }

    mapping = {
        'idl.cs.em': '%s.cs',
        'idl.c.em': '%s_s.c'
    }

    additional_context = {
        'get_field_name' : get_field_name,
        'get_dotnet_type' : get_dotnet_type,
        'constant_value_to_dotnet' : constant_value_to_dotnet,
        'get_c_type' : get_c_type,
        'get_marshal_type' : get_marshal_type,
        'get_marshal_array_type' : get_marshal_array_type,
        'get_csbuild_tool': cs_build_tool
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
}


def get_marshal_type(type_):
    if isinstance(type_, BasicType):
        return BASIC_IDL_TYPES_TO_MARSHAL[type_.typename]
    if isinstance(type_, AbstractString):
        return 'LPStr'
    assert False, "unsupported marshal type '%s'" % type_


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
}


def get_marshal_array_type(type_):
    if isinstance(type_, (AbstractSequence, Array)):
        if isinstance(type_.value_type, AbstractString):
            return 'IntPtr'
        return BASIC_IDL_TYPES_TO_MARSHAL_ARRAY[type_.value_type.typename]
    assert False, "unsupported marshal array type '%s'" % type_


BASIC_IDL_TYPES_TO_DOTNET = {
    'float' : 'float',
    'double' : 'double',
    'long double': 'double',
    'char' : 'char',
    'wchar' : 'ushort',
    'boolean' : 'bool',
    'octet' : 'byte',
    'uint8' : 'byte',
    'int8' : 'sbyte',
    'uint16' : 'ushort',
    'int16' : 'short',
    'uint32' : 'uint',
    'int32' : 'int',
    'uint64' : 'ulong',
    'int64' : 'long',
}


def get_dotnet_type(type_):
    if isinstance(type_, AbstractGenericString):
        return 'System.String'

    if isinstance(type_, NamespacedType):
        return ".".join(type_.namespaced_name())

    if isinstance(type_, NamedType):
        return type_.name

    if isinstance(type_, (AbstractSequence, Array)):
        return get_dotnet_type(type_.value_type) + "[]"

    if isinstance(type_, BasicType):
        return BASIC_IDL_TYPES_TO_DOTNET[type_.typename]

    assert False, "unknown type '%s'" % type_


def get_field_name(type_name, field_name, class_name):
    ucased_name = field_name.capitalize()
    if (ucased_name == type_name) or (ucased_name == class_name):
        return "{0}_".format(ucased_name)
    return ucased_name
