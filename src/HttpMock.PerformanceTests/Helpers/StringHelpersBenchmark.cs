using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace HttpMock.PerformanceTests.Helpers
{
    [MemoryDiagnoser(true)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class StringHelpersBenchmark
    {
        private static readonly string Url = "/domain-name/api/v2?param1=value&param2=value2";

        private const char slash = '/';

        [Benchmark]
        public void ForLoop()
        {
            var count = 0;
            var span = Url.AsSpan();
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == slash)
                    count++;
            }
        }

        [Benchmark(Baseline = true)]
        public void ForEachLoop()
        {
            var count = 0;
            var span = Url.AsSpan();

            foreach (var c in span)
            {
                if (c == slash)
                    count++;
            }
        }

        [Benchmark]
        public void IndexOfWhileLoop()
        {
            var count = 0;
            var span = Url.AsSpan();

            var currentSpan = span;
            int nextIndex;
            while ((nextIndex = currentSpan.IndexOf(slash)) != -1)
            {
                count++;
                currentSpan = currentSpan[(nextIndex + 1)..];
            }
        }
    }
}
