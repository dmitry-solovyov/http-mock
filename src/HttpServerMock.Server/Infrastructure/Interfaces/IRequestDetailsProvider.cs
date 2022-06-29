using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetailsProvider
    {
        IRequestDetails? GetRequestDetails(HttpContext httpContext);
    }
}