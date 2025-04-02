using BenchmarkDotNet.Running;
using HttpMock.PerformanceTests.Helpers;

namespace HttpMock.PerformanceTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<StringHelpersBenchmark>();
            //var summary = BenchmarkRunner.Run<StringBenchmark>();
            //var summary = BenchmarkRunner.Run<SplitByBenchmark>();
            var summary = BenchmarkRunner.Run<PathStringHelperBenchmark>();
            //var summary = BenchmarkRunner.Run<GetOccurrencesCountBenchmark>();
        }
    }
}
