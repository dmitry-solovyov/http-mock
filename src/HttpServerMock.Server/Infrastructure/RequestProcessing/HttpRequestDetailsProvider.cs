using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mime;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public class HttpRequestDetailsProvider : IHttpRequestDetailsProvider
{
    private const string DefaultContentType = MediaTypeNames.Application.Json;

    public bool TryGetRequestDetails(HttpContext httpContext, out HttpRequestDetails requestDetails)
    {
        var request = httpContext?.Request;
        if (request == null)
        {
            requestDetails = default;
            return false;
        }

        requestDetails = new HttpRequestDetails(
            request.Method,
            request.GetDisplayUrl(),
            request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            httpContext?.Connection.RemoteIpAddress?.ToString(),
            request.ContentType ?? DefaultContentType
        );

        return true;
    }
}