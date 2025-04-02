using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using HttpMock.Extensions;

namespace HttpMock.PerformanceTests.Extensions;

[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
[MemoryDiagnoser(true)]
public class GetOccurrencesCountBenchmark
{
    private static readonly string Url = "/part1/part2/part3/part4/part1/part2/part3/part4";

    [Benchmark(Baseline = true)]
    public void Count()
    {
        var url = Url.AsSpan();
        url.Count('/');
    }

    [Benchmark]
    public void GetOccurrencesCount_IndexOfAfter()
    {
        var url = Url.AsSpan();
        var count = 0;
        var currentPosition = 0;

        while ((currentPosition = url.IndexOfAfter('/', currentPosition, false)) != -1)
        {
            count++;
            currentPosition++;
        }
    }
}
