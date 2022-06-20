using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandlerRouter
    {
        RequestHandlerContext GetHandler(HttpContext httpContext);
    }
}