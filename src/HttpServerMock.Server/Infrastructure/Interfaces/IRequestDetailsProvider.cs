using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetailsProvider
    {
        bool TryGetRequestDetails(HttpContext httpContext, out RequestDetails requestDetails);
    }
}