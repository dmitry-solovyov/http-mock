namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

public interface IRequestHistoryStorage
{
    void Clear();

    int CurrentItemsCount { get; }

    MockedRequestDefinition GetMockedRequestWithDefinition(ref HttpRequestDetails requestDetails);
}