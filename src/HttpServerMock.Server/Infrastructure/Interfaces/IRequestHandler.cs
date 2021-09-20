using HttpServerMock.RequestDefinitions;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandler
    {
        Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken);
    }
}
