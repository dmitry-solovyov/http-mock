using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandler
    {
        ValueTask<Models.ResponseDetails> Execute(RequestDetails requestDetails, CancellationToken cancellationToken);
    }
}