using FluentAssertions;
using HttpMock.RequestProcessing;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HttpMock.Tests.RequestProcessing;

public class RequestDetailsProviderTests
{
    private const string TestDomainName = "testDomain";

    [Fact]
    public void TryGetRequestDetails_HttpContextIsEmpty_ExpectDefaultValuesAssigned()
    {
        var httpContext = GetEmptyHttpContextMock();

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.HttpMethod.Should().Be(HttpMethodType.None);
        requestDetails.ContentType.Should().BeEmpty();
        requestDetails.CommandName.Should().BeNull();
        requestDetails.QueryPath.Should().Be("/");
        requestDetails.Domain.Should().BeEmpty();
    }

    [Theory]
    [InlineData("get", "", "/", "", null)]
    [InlineData("post", "", "/", "", null)]
    [InlineData("get", "test-domain", "/", "", null)]
    [InlineData("get", "test-domain", "/api/endpoint", "", null)]
    [InlineData("get", "test-domain", "/api/endpoint", "?param1=@value1", null)]
    [InlineData("delete", "test-domain", "/", "", "test-command")]
    public void TryGetRequestDetails_ExpectValidRequestDetails(
        string httpMethod, string domain, string path, string queryString, string? commandName)
    {
        var httpContext = GetHttpContextMock(
            httpMethod, domain, path, queryString, commandName);

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.HttpMethod.Should().Be(HttpMethodTypeParser.Parse(httpMethod));
        requestDetails.Domain.Should().Be(domain);
        requestDetails.QueryPath.Should().Be($"{path}{queryString}");
        requestDetails.CommandName.Should().Be(commandName);
    }

    [Theory]
    [InlineData("/test-domain/api/endpoint", "test-domain")]
    [InlineData("/", "")]
    [InlineData("//", "")]
    public void TryGetRequestDetails_ExpectDomainExtractedFromUrl(string queryPath, string expectedDomain)
    {
        var httpContext = GetHttpContextMock("get", "", queryPath, "");

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.Domain.Should().Be(expectedDomain);
    }

    private RequestDetailsProvider CreateRequestDetailsProvider() => new();

    private static Mock<HttpContext> GetEmptyHttpContextMock()
    {
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(x => x.Method).Returns(string.Empty);
        httpRequestMock.Setup(x => x.ContentType).Returns(string.Empty);
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.Path).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString(string.Empty));
        httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);
        return httpContextMock;
    }

    private static Mock<HttpContext> GetHttpContextMock(string httpMethod = "GET", string domain = TestDomainName, string path = "/api/endpoint", string queryString = "?param1=value1&param2=@value2", string? commandName = null)
    {
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(x => x.Method).Returns(httpMethod);
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.Path).Returns(new PathString($"{(string.IsNullOrEmpty(domain) ? string.Empty : "/" + domain)}{path}"));
        httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString(queryString));

        if (string.IsNullOrEmpty(commandName))
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
        }
        else
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary
            {
                { "x-httpmock-command", new(commandName) }
            });
        }
        httpRequestMock.Setup(x => x.ContentType).Returns(string.Empty);

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);
        return httpContextMock;
    }
}