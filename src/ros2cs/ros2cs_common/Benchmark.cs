using System;
using System.Diagnostics;

namespace ROS2
{
  /// <summary> An utility class for simple code block execution time measurement </summary>
  /// <code>
  /// /* example use */
  /// using (var bench = new Benchmark("name_to_show_in_logs"))
  /// {
  ///   [code to benchmark]
  /// }
  /// </code>
  public class Benchmark : IDisposable
  {
    private readonly Stopwatch timer = new Stopwatch();
    private readonly string benchmarkName;

    public Benchmark(string benchmarkName)
    {
      this.benchmarkName = benchmarkName;
      timer.Start();
    }

    public void Dispose()
    {
      timer.Stop();
      Console.WriteLine($"{benchmarkName} {timer.ElapsedTicks} ticks ({timer.ElapsedMilliseconds} ms)");
    }
  }
}  // namespace ROS2
