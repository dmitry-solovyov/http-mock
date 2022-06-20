using HttpServerMock.RequestDefinitions;
using Microsoft.AspNetCore.Http;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetailsProvider
    {
        IRequestDetails? GetRequestDetails(HttpContext httpContext);
    }
}