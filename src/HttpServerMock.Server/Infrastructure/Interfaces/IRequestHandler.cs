using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandler
    {
        Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken);
    }
}