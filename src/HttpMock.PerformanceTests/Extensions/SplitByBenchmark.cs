using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using HttpMock.Extensions;

namespace HttpMock.PerformanceTests.Extensions;

[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
[CategoriesColumn]
[MemoryDiagnoser(true)]
public class SplitByBenchmark
{
    private static readonly string Url = "/part1/part2/part3/part4";

    [Benchmark(Baseline = true)]
    public void SplitByOld()
    {
        var url = Url.AsSpan();
        url.SplitByOld('/');
    }

    [Benchmark]
    public void SplitBy()
    {
        var url = Url.AsSpan();
        url.SplitBy('/');
    }
}
