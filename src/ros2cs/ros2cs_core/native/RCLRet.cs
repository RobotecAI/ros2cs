namespace ROS2 {
    //Codes returned from rcl C library as for Dashing
    public enum RCLReturnEnum
  	{
      RCL_RET_OK = 0,
      RCL_RET_ERROR = 1,
      RCL_RET_TIMEOUT = 2,

      RCL_RET_BAD_ALLOC = 10,
      RCL_RET_INVALID_ARGUMENT = 11,
      RCL_RET_INCORRECT_RMW_IMPLEMENTATION = 12,

      // rcl specific ret codes start at 100
      RCL_RET_ALREADY_INIT = 100,
      RCL_RET_NOT_INIT = 101,
      RCL_RET_MISMATCHED_RMW_ID = 102,
      RCL_RET_TOPIC_NAME_INVALID = 103,
      RCL_RET_SERVICE_NAME_INVALID = 104,
      RCL_RET_UNKNOWN_SUBSTITUTION = 105,
      RCL_RET_ALREADY_SHUTDOWN = 106,
      // rcl node specific ret codes in 2XX
      RCL_RET_NODE_INVALID = 200,
      RCL_RET_NODE_INVALID_NAME = 201,
      RCL_RET_NODE_INVALID_NAMESPACE = 202,

      // rcl publisher specific ret codes in 3XX
      RCL_RET_PUBLISHER_INVALID = 300,
      // rcl subscription specific ret codes in 4XX
      RCL_RET_SUBSCRIPTION_INVALID = 400,
      RCL_RET_SUBSCRIPTION_TAKE_FAILED = 401,
      // rcl service client specific ret codes in 5XX
      RCL_RET_CLIENT_INVALID = 500,
      RCL_RET_CLIENT_TAKE_FAILED = 501,
      // rcl service server specific ret codes in 6XX
      RCL_RET_SERVICE_INVALID = 600,
      RCL_RET_SERIVCE_TAKE_FAILD = 601,
      // rcl guard condition specific ret codes in 7XX
      // rcl timer specific ret codes in 8XX
      RCL_RET_TIMER_INVALID = 800,
      RCL_RET_TIMER_CANCELED = 801,
      // rcl wait and wait set specific ret codes in 9XX
      RCL_RET_WAIT_SET_INVALID = 900,
      RCL_RET_WAIT_SET_EMPTY = 901,
      RCL_RET_WAIT_SET_FULL = 902,

      // rcl params, logs and events
      RCL_RET_INVALID_REMAP_RULE = 1001,
      RCL_RET_WRONG_LEXME = 1002,
      RCL_RET_INVALID_PARAM_RULE = 1010,
      RCL_RET_INVALID_LOG_LEVEL_RULE = 1020,
      RCL_RET_INVALID_EVENT_ID = 2000,
      RCL_RET_EVENT_TAKE_FAILER = 2001
  	}
}
