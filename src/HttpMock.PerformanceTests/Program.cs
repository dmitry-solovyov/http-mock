using BenchmarkDotNet.Running;
using HttpMock.PerformanceTests.StringTests;

namespace HttpMock.PerformanceTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<StringBenchmark>();
            //var summary = BenchmarkRunner.Run<UrlStringHelperBenchmark>();
        }
    }
}
