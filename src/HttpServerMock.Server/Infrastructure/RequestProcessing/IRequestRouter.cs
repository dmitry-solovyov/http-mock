using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public interface IRequestRouter
{
    bool TryGetHandlerContext(HttpContext httpContext, out RequestHandlerContext requestHandlerContext);
}