using FluentAssertions;
using HttpMock.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HttpMock.Tests.Extensions
{
    public class HttpRequestExtensionsTests
    {
        [Theory]
        [InlineData(null, "application/json")]
        [InlineData("", "application/json")]
        [InlineData("content-type", "content-type")]
        [InlineData("content-type;encoding", "content-type")]
        public void GetNormalizedContentType(string? contentType, string expectedResult)
        {
            var httpRequestMock = CreateHttpRequestMock(contentType);

            var result = HttpRequestExtensions.GetNormalizedContentType(httpRequestMock.Object);

            result.Should().Be(expectedResult);
        }

        private static Mock<HttpRequest> CreateHttpRequestMock(string? contentType)
        {
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(x => x.Method).Returns(string.Empty);
            httpRequestMock.Setup(x => x.ContentType).Returns(contentType);
            httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
            httpRequestMock.Setup(x => x.Path).Returns(new PathString(string.Empty));
            httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString(string.Empty));
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
            return httpRequestMock;
        }
    }
}
