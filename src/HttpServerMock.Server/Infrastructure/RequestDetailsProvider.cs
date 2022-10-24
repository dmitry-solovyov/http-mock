using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mime;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDetailsProvider : IRequestDetailsProvider
    {
        private const string DefaultContentType = MediaTypeNames.Application.Json;

        public bool TryGetRequestDetails(HttpContext httpContext, out RequestDetails requestDetails)
        {
            var request = httpContext?.Request;
            if (request == null)
            {
                requestDetails = default(RequestDetails);
                return false;
            }

            requestDetails = new RequestDetails(
                request.Method,
                request.GetDisplayUrl(),
                request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                request.ContentType ?? DefaultContentType
            );

            return true;
        }
    }
}