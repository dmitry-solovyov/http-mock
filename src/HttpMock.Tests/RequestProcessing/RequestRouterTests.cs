using FluentAssertions;
using HttpMock.RequestHandlers.CommandRequestHandlers;
using HttpMock.RequestProcessing;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpMock.Tests.RequestProcessing;

public class RequestRouterTests
{
    private readonly Mock<IMockedRequestHandler> _mockedRequestHandlerMock;
    private readonly Mock<IUnknownCommandHandler> _unknownCommandHandlerMock;
    private readonly Mock<ICommandRequestHandler> _configurationCommandHandlerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly RequestRouter _router;

    public RequestRouterTests()
    {
        _mockedRequestHandlerMock = new();
        _unknownCommandHandlerMock = new();
        _configurationCommandHandlerMock = new();

        _serviceProviderMock = new();
        _serviceProviderMock
            .Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IMockedRequestHandler))))
            .Returns(_mockedRequestHandlerMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IUnknownCommandHandler))))
            .Returns(_unknownCommandHandlerMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.Is<Type>(t => t == typeof(ConfigurationCommandHandler))))
            .Returns(_configurationCommandHandlerMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.Is<Type>(t => t != typeof(IMockedRequestHandler) && 
                                                      t != typeof(ConfigurationCommandHandler) && 
                                                      t != typeof(IUnknownCommandHandler))))
            .Returns(_mockedRequestHandlerMock.Object);

        _router = new();
    }

    [Fact]
    public async Task TryGetCommandRequestDetails_HttpContextIsEmpty_ExpectDefaultValuesAssigned()
    {
        var httpContext = GetEmptyHttpContextMock();

        var result = await _router.TryExecuteRequestHandler(httpContext.Object);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("get", "/", "", "/")]
    [InlineData("post", "/", "", "/")]
    [InlineData("get", "/api/endpoint", "", "/api/endpoint")]
    [InlineData("get", "/api/endpoint", "?param1=@value1", "/api/endpoint?param1=@value1")]
    public async Task TryGetRequestDetails_ExpectValidRequestDetails(
        string httpMethod, string path, string queryString, string expectedPath)
    {
        var httpContext = GetHttpContextMock(httpMethod, path, queryString);

        var result = await _router.TryExecuteRequestHandler(httpContext.Object);

        result.Should().BeTrue();

        var parsedHttpMethod = HttpMethodTypeParser.Parse(httpMethod.AsSpan());
        _mockedRequestHandlerMock.Verify(x => x.Execute(
            It.Is<Models.RequestDetails>(r => r.HttpMethod == parsedHttpMethod && r.Path == expectedPath),
            It.IsAny<HttpResponse>(), It.IsAny<CancellationToken>()));
    }

    [Theory]
    [InlineData("get", "test-command")]
    public async Task TryGetCommandRequestDetails_ExpectValidRequestDetails(string httpMethod, string commandName)
    {
        var httpContext = GetHttpContextMockForCommand(httpMethod, commandName);

        var result = await _router.TryExecuteRequestHandler(httpContext.Object);

        result.Should().BeTrue();

        _unknownCommandHandlerMock.Verify(x => x.Execute(
            It.Is<Models.CommandRequestDetails>(r => r.HttpMethod == HttpMethodType.Get && r.CommandName == commandName),
            It.IsAny<HttpResponse>(), It.IsAny<CancellationToken>()));
    }

    [Theory]
    [InlineData("/api/endpoint")]
    [InlineData("/")]
    [InlineData("//")]
    public async Task TryGetRequestDetails_ExpectDomainExtractedFromUrl(string path)
    {
        var httpContext = GetHttpContextMock("get", path, "");

        var result = await _router.TryExecuteRequestHandler(httpContext.Object);

        result.Should().BeTrue();
    }

    private Mock<HttpContext> GetEmptyHttpContextMock()
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
        httpContextMock.Setup(x => x.RequestServices).Returns(_serviceProviderMock.Object);
        return httpContextMock;
    }

    private Mock<HttpContext> GetHttpContextMockForCommand(string httpMethod, string commandName)
    {
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(x => x.Method).Returns(httpMethod);
        httpRequestMock.Setup(x => x.PathBase).Returns(new PathString(string.Empty));
        httpRequestMock.Setup(x => x.Path).Returns(new PathString("/"));

        if (string.IsNullOrEmpty(commandName))
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary());
        }
        else
        {
            httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary
            {
                { "x-httpMock-command", new(commandName) }
            });
        }
        httpRequestMock.Setup(x => x.ContentType).Returns(string.Empty);

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);
        httpContextMock.Setup(x => x.RequestServices).Returns(_serviceProviderMock.Object);
        return httpContextMock;
    }

    private Mock<HttpContext> GetHttpContextMock(string httpMethod, string path, string queryString)
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
        httpContextMock.Setup(x => x.RequestServices).Returns(_serviceProviderMock.Object);
        return httpContextMock;
    }
}