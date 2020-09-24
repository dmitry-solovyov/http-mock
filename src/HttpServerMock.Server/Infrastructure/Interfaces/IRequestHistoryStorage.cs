using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHistoryStorage
    {
        void Clear();
        int CurrentItemsCount { get; }
        MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails);
    }
}