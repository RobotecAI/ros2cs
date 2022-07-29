// generated from rosidl_generator_cs/resource/idl.c.em
// with input from @(package_name):@(interface_path)
// generated code does not contain a copyright notice
@
@#######################################################################
@# EmPy template for generating <idl>.c files
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

@#######################################################################
@# Handle message
@#######################################################################
@{
from rosidl_parser.definition import Message
}@
@[for message in content.get_elements_of_type(Message)]@
@{
TEMPLATE(
    'msg.c.em',
    package_name=package_name, interface_path=interface_path,
    message=message, include_parts=include_parts,
    get_c_type=get_c_type)
}@

@[end for]@
@# TODO (adamdbrw): Add services and actions

@#######################################################################
@# Handle service
@#######################################################################
@{
from rosidl_parser.definition import Service
}@
@[if include_parts[1] == "srv"]@
@[for service in content.get_elements_of_type(Service)]@
@{
TEMPLATE(
    'srv.c.em',
    package_name=package_name, interface_path=interface_path,service=service,
    message=service.request_message, include_parts=include_parts,
    get_c_type=get_c_type)
}@

@{
TEMPLATE(
    'srv.c.em',
    package_name=package_name, interface_path=interface_path,service=service,
    message=service.response_message, include_parts=include_parts,
    get_c_type=get_c_type)
}@
@[end for]@
@[end if]@
@# // endif

