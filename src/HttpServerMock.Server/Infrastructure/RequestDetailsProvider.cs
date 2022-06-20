using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDetailsProvider : IRequestDetailsProvider
    {
        public IRequestDetails? GetRequestDetails(HttpContext httpContext)
        {
            var request = httpContext?.Request;
            if (request == null)
                return null;

            var result = new RequestDetails(
                request.Method,
                request.GetDisplayUrl(),
                request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
                httpContext?.Connection.RemoteIpAddress?.ToString(),
                request.ContentType
            );

            return result;
        }
    }
}