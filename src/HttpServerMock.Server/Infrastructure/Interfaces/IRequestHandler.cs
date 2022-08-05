using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandler
    {
        Task<IResponseDetails> Execute(RequestDetails requestDetails, CancellationToken cancellationToken);
    }
}