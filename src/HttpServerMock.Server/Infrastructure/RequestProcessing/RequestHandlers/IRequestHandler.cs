using HttpServerMock.Server.Infrastructure.RequestProcessing;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers
{
    public interface IRequestHandler
    {
        ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken);
    }
}