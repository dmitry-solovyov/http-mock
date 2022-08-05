using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandlerRouter
    {
        bool TryGetHandler(HttpContext httpContext, out RequestHandlerContext requestHandlerContext);
    }
}