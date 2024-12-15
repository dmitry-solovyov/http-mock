using FluentAssertions;
using HttpMock.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HttpMock.Tests.Helpers
{
    public class UrlStringHelperTests
    {
        [Fact]
        public void SimpleTest()
        {
            var url = "/domain-name/api/v2?param1=value&param2=value2";
            var trimmedUrl = url.TrimStart('/');
            var domain = trimmedUrl.Substring(0, trimmedUrl.IndexOf('/'));
            trimmedUrl = trimmedUrl.Substring(trimmedUrl.IndexOf('/'));
            var urlParts = trimmedUrl.Split('?');
            trimmedUrl = urlParts[0];

            List<(string Name, string Value)>? urlParameters = new();
            var parametersSections = urlParts[1].Split('&');
            foreach (var parameterSection in parametersSections)
            {
                var paramParts = parameterSection.Split('=');
                urlParameters.Add((paramParts[0], paramParts[1]));
            }

            var result = (domain, trimmedUrl, urlParameters);

            result.trimmedUrl.Should().NotBeEmpty();
            result.urlParameters.Should().NotBeNull();
        }

        [Fact]
        public void GetPathRef_EmptyUrl_ExpectEmptyUrl()
        {
            var span = string.Empty.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, false);

            result.Path.Range.Should().Be(0..0);
        }

        [Fact]
        public void GetPathRef_SimpleUrl_ExpectEmptyUrl()
        {
            var url = "/";
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, false);

            url[result.Path.Range].Should().Be("/");
        }

        [Fact]
        public void GetPathRef_UrlWithoutQuery_ExpectEmptyParameters()
        {
            var url = "/api/v2";
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, false);

            url[result.Path.Range].Should().Be("/api/v2");
        }

        [Fact]
        public void GetPathRef_UrlWithQuery_ExpectParsedCorrectly()
        {
            var url = "/api/v2?param1=value&param2=value2";
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, false);

            url[result.Path.Range].Should().Be("/api/v2");
        }

        [Fact]
        public void GetPathRef_UrlStartPosition_ExpectParsedCorrectly()
        {
            var url = "/domain-name/api/v2?param1=value&param2=value2";
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, true);

            url[result.Path.Range].Should().Be("/api/v2");
        }

        [Fact]
        public void GetQueryParametersRef_UrlWithParameters_ExpectParsedCorrectly()
        {
            var url = "/api/v2?param1=value&param2=value2";
            var span = url.AsSpan();
            var result = PathStringHelper.GetQueryParametersRef(in span);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(2);
            url[result![0].Name.Range].Should().Be("param1");
            url[result[0].Name.Range].Should().Be("param1");
            url[result[0].Value.Range].Should().Be("value");
            url[result[1].Name.Range].Should().Be("param2");
            url[result[1].Value.Range].Should().Be("value2");
        }

        [Fact]
        public void GetQueryParametersRef_UrlEmptyValueInsideParameters_ExpectParsedCorrectly()
        {
            var url = "/api/v2?param1=&param2=value2";
            var span = url.AsSpan();
            var result = PathStringHelper.GetQueryParametersRef(in span);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(2);
            url[result![0].Name.Range].Should().Be("param1");
            url[result[0].Value.Range].Should().Be(string.Empty);
            url[result[1].Name.Range].Should().Be("param2");
            url[result[1].Value.Range].Should().Be("value2");
        }

        [Fact]
        public void GetQueryParametersRef_UrlEmptyValueAtTheEndOfParameters_ExpectParsedCorrectly()
        {
            var url = "/api/v2?param1=&param2=value2&param3=";
            var span = url.AsSpan();
            var result = PathStringHelper.GetQueryParametersRef(in span);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            url[result![0].Name.Range].Should().Be("param1");
            url[result[0].Value.Range].Should().Be(string.Empty);
            url[result[1].Name.Range].Should().Be("param2");
            url[result[1].Value.Range].Should().Be("value2");
            url[result[2].Name.Range].Should().Be("param3");
            url[result[2].Value.Range].Should().Be(string.Empty);
        }

        [Theory]
        [MemberData(nameof(TestCases), MemberType = typeof(UrlStringHelperTests))]
        public void GetUrlPartsWithDomain_CheckTestCases(string url)
        {
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span, false);

            result.Should().NotBeNull();
        }

        public static IEnumerable<object[]> TestCases => _urlTestCases.Select(x => new object[] { x });

        private static readonly string[] _urlTestCases = new string[]
        {
            "/page",                                  // Single slash, simple path
            "/products/list",                         // Multiple slashes, simple path
            "/category/electronics/phones",           // Multiple slashes, deeper path
            "/search?q=mobile",                       // Single parameter
            "/search?q=smartphone&sort=price",        // Two parameters
            "/profile?userId=12345",                  // Numeric parameter
            "/settings?theme=dark&lang=en",           // Multiple parameters, text values
            "/item/details?id=567&ref=promo",         // Mixed text and numeric parameters
            "/cart/add?item=abc123&qty=2",            // Multiple parameters with a mix of letters and numbers
            "/product/view?item=789&category=home/kitchen", // Path and parameter with slash in value
            "/checkout?payment=creditcard&type=visa", // Parameters with special characters
            "/account/settings?notify=true&promo=false", // Boolean parameters
            "/images/gallery?name=sunset%20view",     // Encoded space in parameter value
            "/download?file=report.pdf",              // File parameter
            "/update?key=API%23KEY123",               // Parameter with encoded special character '#'
            "/path/with%2Fencoded%2Fslash",           // Encoded forward slashes in path
            "/blog/post?id=42&title=hello-world",     // Numeric and text parameters
            "/order/confirm?orderId=1001&status=shipped&tracking=TRK%23123", // Three parameters with encoded special characters
            "/search?query=cat&filter=color:black",   // Colon in parameter value
            "/path/to/resource?param1=value1&param2=value%3A2&param3=value%26with%26specials", // Multiple encoded special characters
            "/action/submit?flag&user=admin",          // Parameter without value, alongside one with a value
            "/process?token=abc123&debug",             // Parameter with a value and one without
        };

    }
}
