# Ros2Cs Examples

## Examples

*  `ROS2Listener` / `ROS2Talker` - simple string subscriber/publisher test.
*  `ROS2PerformanceListener` / `ROS2PerformanceTalker` - performance test using PointClound2 data.

## Simple subscriber/listener

1.  Build project:
 
    ```bash
    ./build.sh
    ```

2.  Run listener:
  
    ```bash
    ros2 run ros2cs_examples ros2cs_talker
    ```

3.  Run talker:

    ```bash
    ros2 run ros2cs_examples ros2cs_talker
    ```

Listener will print out `"I heard: [Hello World: X]` messages which are being send by a talker. 

## Performance test

1.  Build project:
 
    ```bash
    ./build.sh
    ```

2.  Run talker:
  
    ```bash
    ros2 run ros2cs_examples ros2cs_performance_talker
    ```

3.  When asked, set desired `PointCloud2` data size (number of points),

4.  Run listener:

    ```bash
    ros2 run ros2cs_examples ros2cs_performance_listener
    ```

5. When asked, set desired sample size (number of messages).

After receiving the desired number of samples, listener will print out average latency and its `Latency of sample size X - avg: Ys, std dev: Zs`

### Example results

Hardware spec:
*  **CPU**: i7 4970k
*  **MEM**: 16GB Ram

| PointCloud size | Sample size | Average rate [Hz] | Average latency [s] | Latency std dev [s] |
|-|-|-|-|-|-|
| 100 000 | 5000 | 719.308 | 0.001591 | 0.000306 |
| 1 000 000 | 500 | 77.025 | 0.022677 | 0.001607 |