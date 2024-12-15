using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using HttpMock.Helpers;

namespace HttpMock.PerformanceTests.Helpers
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical)]
    [CategoriesColumn]
    [MemoryDiagnoser(true)]
    public class UrlStringHelperBenchmark
    {
        private static readonly string Url = "/domain-name/api/v2?param1=value&param2=value2&param3=";

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Path")]
        public void PartsWithStringFunctionsWithParameters()
        {
            var url = Url.TrimStart('/');
            var domain = url.Substring(0, url.IndexOf('/'));
            url = url.Substring(url.IndexOf('/'));
            var urlParts = url.Split('?');
            url = urlParts[0];

            List<(string Name, string Value)>? urlParameters = new();
            var parametersSections = urlParts[1].Split('&');
            foreach (var parameterSection in parametersSections)
            {
                var paramParts = parameterSection.Split('=');
                urlParameters.Add((paramParts[0], paramParts[1]));
            }

            var _ = (domain, url, urlParameters);
        }

        [Benchmark]
        [BenchmarkCategory("Path")]
        public void PartsWithStringFunctions()
        {
            var url = Url.TrimStart('/');
            var domain = url.Substring(0, url.IndexOf('/'));
            url = url.Substring(url.IndexOf('/'));
            var urlParts = url.Split('?');
            url = urlParts[0];
            var _ = (domain, url);
        }

        [Benchmark]
        [BenchmarkCategory("Path")]
        public void GetUrlParts_New()
        {
            var span = Url.AsSpan();
            PathStringHelper.GetPathParts(ref span, false);
        }

        [Benchmark]
        [BenchmarkCategory("QueryParameters")]
        public void GetQueryParametersRef()
        {
            var span = Url.AsSpan();
            PathStringHelper.GetQueryParametersRef(ref span);
        }
    }
}
