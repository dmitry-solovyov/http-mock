using FluentAssertions;
using HttpMock.RequestProcessing;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace HttpMock.Tests.RequestProcessing;

public class RequestDetailsProviderTests
{
    [Fact]
    public void TryGetCommandRequestDetails_HttpContextIsEmpty_ExpectDefaultValuesAssigned()
    {
        var httpContext = GetEmptyHttpContextMock();

        var result = new RequestDetailsProvider().TryGetCommandRequestDetails(httpContext.Object, out var commandRequestDetails);

        result.Should().BeFalse();

        commandRequestDetails.HttpMethod.Should().Be(HttpMethodType.None);
        commandRequestDetails.ContentType.Should().BeNull();
        commandRequestDetails.CommandName.Should().BeNull();
        commandRequestDetails.Domain.Should().BeNull();
    }

    [Fact]
    public void TryGetRequestDetails_HttpContextIsEmpty_ExpectDefaultValuesAssigned()
    {
        var httpContext = GetEmptyHttpContextMock();

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.HttpMethod.Should().Be(HttpMethodType.None);
        requestDetails.Path.Should().BeEmpty();
        requestDetails.Domain.Should().BeEmpty();
    }

    [Theory]
    [InlineData("get", "/", "", "", "", default)]
    [InlineData("post", "/", "", "", "", default)]
    [InlineData("get", "/test-domain", "", "test-domain", "", default)]
    [InlineData("get", "/test-domain/api/endpoint", "", "test-domain", "/api/endpoint", default)]
    [InlineData("get", "/test-domain/api/endpoint", "?param1=@value1", "test-domain", "/api/endpoint?param1=@value1", "/api/endpoint")]
    [InlineData("delete", "/test-domain", "", "test-domain", "", default)]
    [InlineData("delete", "/test-domain/", "", "test-domain", "/", default)]
    public void TryGetRequestDetails_ExpectValidRequestDetails(
        string httpMethod, string path, string queryString, string expectedDomain, string expectedPath, string? expectedPathRange)
    {
        var httpContext = GetHttpContextMock(httpMethod, path, queryString);

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.HttpMethod.Should().Be(HttpMethodTypeParser.Parse(httpMethod.AsSpan()));
        requestDetails.Domain.Should().Be(expectedDomain);
        requestDetails.Path.Should().Be(expectedPath);
        requestDetails.Path[requestDetails.PathPart.Range].Should().Be(expectedPathRange ?? expectedPath);
    }

    [Theory]
    [InlineData("get", "test-command", "test-domain")]
    [InlineData("get", "test-command", "")]
    public void TryGetCommandRequestDetails_ExpectValidRequestDetails(string httpMethod, string commandName, string domain)
    {
        var httpContext = GetHttpContextMockForCommand(httpMethod, commandName, domain);

        var result = new RequestDetailsProvider().TryGetCommandRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.HttpMethod.Should().Be(HttpMethodTypeParser.Parse(httpMethod.AsSpan()));
        requestDetails.Domain.Should().Be(domain);
        requestDetails.CommandName.Should().Be(commandName);
    }

    [Theory]
    [InlineData("/test-domain/api/endpoint", "test-domain")]
    [InlineData("/", "")]
    [InlineData("//", "/")]
    public void TryGetRequestDetails_ExpectDomainExtractedFromUrl(string path, string expectedDomain)
    {
        var httpContext = GetHttpContextMock("get", path, "");

        var result = new RequestDetailsProvider().TryGetRequestDetails(httpContext.Object, out var requestDetails);

        result.Should().BeTrue();

        requestDetails.Domain.Should().Be(expectedDomain);
    }

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

    private static Mock<HttpContext> GetHttpContextMockForCommand(string httpMethod, string commandName, string domain)
    {
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(x => x.Method).Returns(httpMethod);
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.Path).Returns(new PathString("/"));

        if (string.IsNullOrEmpty(commandName) && string.IsNullOrEmpty(domain))
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
        }
        else
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary
            {
                { "x-httpMock-domain", new(domain) },
                { "x-httpMock-command", new(commandName) }
            });
        }
        httpRequestMock.Setup(x => x.ContentType).Returns(string.Empty);

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);
        return httpContextMock;
    }

    private static Mock<HttpContext> GetHttpContextMock(string httpMethod, string path, string queryString)
    {
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(x => x.Method).Returns(httpMethod);
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.Path).Returns(new PathString(path));
        httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString(queryString));
        httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
        httpRequestMock.Setup(x => x.ContentType).Returns(string.Empty);

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);
        return httpContextMock;
    }
}