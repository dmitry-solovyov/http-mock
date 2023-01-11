namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public interface IHttpRequestDetailsProvider
{
    bool TryGetRequestDetails(HttpContext httpContext, out HttpRequestDetails requestDetails);
}