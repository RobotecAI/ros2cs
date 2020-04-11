// generated from rosidl_generator_cs/resource/idl.cs.em
// with input from @(package_name):@(interface_path)
// generated code does not contain a copyright notice
@
@#######################################################################
@# EmPy template for generating <idl>.cs files
@#
@# Context:
@#  - package_name (string)
@#  - interface_path (Path relative to the directory named after the package)
@#  - content (IdlContent, list of elements, e.g. Messages or Services)
@#######################################################################
@{
from rosidl_cmake import convert_camel_case_to_lower_case_underscore
include_parts = [package_name] + list(interface_path.parents[0].parts) + \
    [convert_camel_case_to_lower_case_underscore(interface_path.stem)]
include_directives = set()
}@

//TODO (adamdbrw): include depending on what is needed
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ROS2;

@#######################################################################
@# Handle message
@#######################################################################
@{
from rosidl_parser.definition import Message
}@
@[for message in content.get_elements_of_type(Message)]@
@{
TEMPLATE(
    'msg.cs.em',
    package_name=package_name, interface_path=interface_path,
    message=message, include_directives=include_directives,
    get_dotnet_type=get_dotnet_type, get_field_name=get_field_name,
    constant_value_to_dotnet=constant_value_to_dotnet,
    get_c_type=get_c_type, get_marshal_type=get_marshal_type,
    get_marshal_array_type=get_marshal_array_type
    )
}@

@[end for]@
@# TODO (adamdbrw): Add services and actions
