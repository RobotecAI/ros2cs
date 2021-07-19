import traceback
import logging

__all__ = ['import_type_support']

try:
    from .generate_cs_impl import generate_cs
    assert generate_cs
    __all__.append('generate_cs')
except ImportError:
    logger = logging.getLogger('rosidl_generator_cs')
    logger.debug(
        'Failed to import modules for generating C# structures:\n' + traceback.format_exc())
