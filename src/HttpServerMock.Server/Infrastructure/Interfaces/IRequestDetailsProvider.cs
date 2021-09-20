using HttpServerMock.RequestDefinitions;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetailsProvider
    {
        Task<IRequestDetails> GetRequestDetails(CancellationToken cancellationToken);
    }
}