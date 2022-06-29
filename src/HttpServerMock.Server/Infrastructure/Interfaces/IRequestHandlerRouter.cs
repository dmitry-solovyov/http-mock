using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandlerRouter
    {
        RequestHandlerContext GetHandler(HttpContext httpContext);
    }
}