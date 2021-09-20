using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;
using System.Threading;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHistoryStorage
    {
        void Clear();
        int CurrentItemsCount { get; }
        MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails, CancellationToken cancellationToken);
    }
}