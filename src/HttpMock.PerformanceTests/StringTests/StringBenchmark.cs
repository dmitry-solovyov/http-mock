using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Text;

namespace HttpMock.PerformanceTests.StringTests;

[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
[MemoryDiagnoser(true)]
public class StringBenchmark
{
    const string ExMessage = "Some error text!";
    const string ExStackStrace = @"Stack trace:
- line 1
- line 2
- line 3
- line 4
- line 5
";

    [Benchmark(Baseline = true)]
    public void String()
    {
        var _ = $"Unhandled exception {ExMessage}{Environment.NewLine}{ExStackStrace}!";
    }

    [Benchmark]
    public void StringBuilder()
    {
        var sb = new StringBuilder();
        sb.Append("Unhandled exception ").Append(ExMessage).Append(Environment.NewLine).Append(ExStackStrace).Append("!");
        var _ = sb.ToString();
    }
}
