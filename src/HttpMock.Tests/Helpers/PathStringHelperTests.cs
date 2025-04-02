using FluentAssertions;
using HttpMock.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HttpMock.Tests.Helpers
{
    public class PathStringHelperTests
    {
        [Fact]
        public void GetPathParts_EmptyUrl_ExpectEmptyUrl()
        {
            var span = string.Empty.AsSpan();
            var result = PathStringHelper.GetPathParts(in span);

            result.PathWithoutQuery.Segment.IsEmpty.Should().BeTrue();
            result.PathWithoutQuery.Segment.Start.Should().Be(0);
            result.PathWithoutQuery.Segment.End.Should().Be(0);
        }

        [Theory]
        [InlineData("/path1?param1=value1#data", 6, 20)]
        [InlineData("/path1?param1=value1", 6, 20)]
        [InlineData("/path1", 6, 6)]
        [InlineData("/", 1, 1)]
        [InlineData("", 0, 0)]
        public void GetPathSplitterPositions(string inputPath, int expectedPosition1, int expectedPosition2)
        {
            var inputPathSpan = inputPath.AsSpan();

            var (position1, position2) = PathStringHelper.GetPathSplitterPositions(in inputPathSpan);

            position1.Should().Be(expectedPosition1);
            position2.Should().Be(expectedPosition2);
        }

        [Theory]
        [InlineData("/path1/path2/part3?param1=value1", "/path1/path2/part3", "param1=value1")]
        [InlineData("/path1/path2?param1=value1", "/path1/path2", "param1=value1")]
        [InlineData("/path1/path2", "/path1/path2", "")]
        [InlineData("/path1?param1=value1", "/path1", "param1=value1")]
        [InlineData("/path1", "/path1", "")]
        [InlineData("/", "/", "")]
        public void GetPathParts_ExpectParsedCorrectly(string inputPath, string expectedPath, string expectedQuery)
        {
            var inputPathSpan = inputPath.AsSpan();

            var parts = PathStringHelper.GetPathParts(in inputPathSpan);

            inputPath[parts.PathWithoutQuery.Segment.Range].Should().Be(expectedPath);
            inputPath[parts.Query.Segment.Range].Should().Be(expectedQuery);
        }

        [Fact]
        public void GetPathParts_ExpectParametersParsedCorrectly()
        {
            var inputPath = "/path1//@dynamicSection/part3?param1=value1&param2=&param3=@dynamicValue";
            var inputPathSpan = inputPath.AsSpan();

            var parts = PathStringHelper.GetPathParts(in inputPathSpan);

            inputPath[parts.PathWithoutQuery.Segment.Range].Should().Be("/path1//@dynamicSection/part3");
            parts.PathWithoutQuery.Subdirectories.Should().HaveCount(4);
            inputPath[parts.PathWithoutQuery.Subdirectories[0].Segment.Range].Should().Be("path1");
            parts.PathWithoutQuery.Subdirectories[0].IsVariable.Should().BeFalse();
            inputPath[parts.PathWithoutQuery.Subdirectories[1].Segment.Range].Should().Be("");
            parts.PathWithoutQuery.Subdirectories[1].IsVariable.Should().BeFalse();
            inputPath[parts.PathWithoutQuery.Subdirectories[2].Segment.Range].Should().Be("@dynamicSection");
            parts.PathWithoutQuery.Subdirectories[2].IsVariable.Should().BeTrue();
            inputPath[parts.PathWithoutQuery.Subdirectories[3].Segment.Range].Should().Be("part3");
            parts.PathWithoutQuery.Subdirectories[3].IsVariable.Should().BeFalse();

            inputPath[parts.Query.Segment.Range].Should().Be("param1=value1&param2=&param3=@dynamicValue");
            parts.Query.HasParameters.Should().BeTrue();
            parts.Query.Parameters.Should().NotBeNull();
            parts.Query.Parameters.Should().HaveCount(3);

            inputPath[parts.Query.Parameters![0].Segment.Range].Should().Be("param1=value1");
            inputPath[parts.Query.Parameters[0].NameSegment.Range].Should().Be("param1");
            inputPath[parts.Query.Parameters[0].ValueSegment.Range].Should().Be("value1");
            parts.Query.Parameters[0].IsVariable.Should().BeFalse();

            inputPath[parts.Query.Parameters[1].Segment.Range].Should().Be("param2=");
            inputPath[parts.Query.Parameters[1].NameSegment.Range].Should().Be("param2");
            inputPath[parts.Query.Parameters[1].ValueSegment.Range].Should().Be(string.Empty);
            parts.Query.Parameters[1].IsVariable.Should().BeFalse();

            inputPath[parts.Query.Parameters[2].Segment.Range].Should().Be("param3=@dynamicValue");
            inputPath[parts.Query.Parameters[2].NameSegment.Range].Should().Be("param3");
            inputPath[parts.Query.Parameters[2].ValueSegment.Range].Should().Be("@dynamicValue");
            parts.Query.Parameters[2].IsVariable.Should().BeTrue();
        }

        [Theory]
        [InlineData("/path1/path2?param1=value1#fragment", "/path1/path2")]
        [InlineData("/path1/path2?param1=value1", "/path1/path2")]
        [InlineData("/path1/path2", "/path1/path2")]
        [InlineData("/", "/")]
        public void GetPathWithoutQuery(string inputPath, string expectedResult)
        {
            var inputPathSpan = inputPath.AsSpan();

            var pathRange = PathStringHelper.GetPathWithoutQuery(in inputPathSpan);

            inputPath[pathRange].Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(TestCases), MemberType = typeof(PathStringHelperTests))]
        public void GetPathParts_CheckTestCases(string url)
        {
            var span = url.AsSpan();
            var result = PathStringHelper.GetPathParts(in span);

            result.Should().NotBeNull();
        }

        public static IEnumerable<object[]> TestCases => UrlTestCases.Select(x => new object[] { x });

        private static readonly string[] UrlTestCases =
        [
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
        ];
    }
}
