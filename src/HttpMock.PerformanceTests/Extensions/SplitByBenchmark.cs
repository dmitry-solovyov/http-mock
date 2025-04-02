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

    [Benchmark]
    public void SplitByOld()
    {
        var url = Url.AsSpan();
        url.SplitByOld('/');
    }

    [Benchmark(Baseline = true)]
    public void SplitBy()
    {
        var url = Url.AsSpan();
        url.SplitBy('/');
    }

    [Benchmark]
    public void SplitByNew()
    {
        var url = Url.AsSpan();
        url.SplitByNew('/');
    }
}
