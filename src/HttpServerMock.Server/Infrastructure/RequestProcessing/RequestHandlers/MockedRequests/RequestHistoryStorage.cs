using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using System.Collections.Concurrent;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

public class RequestHistoryStorage : IRequestHistoryStorage
{
    private readonly ConcurrentDictionary<string, RequestContext> _requestHistory =
        new ConcurrentDictionary<string, RequestContext>();

    private readonly IConfigurationStorage _requestDefinitionProvider;

    public RequestHistoryStorage(IConfigurationStorage requestDefinitionProvider)
    {
        _requestDefinitionProvider = requestDefinitionProvider;
    }

    public int CurrentItemsCount => _requestHistory.Count;

    public void Clear() => _requestHistory.Clear();

    private static string GetHistoryItemCacheKey(ref HttpRequestDetails requestDetails) => $"{requestDetails.HttpMethod?.ToLower()}_{requestDetails.Url?.ToLower()}";

    public MockedRequestDefinition GetMockedRequestWithDefinition(ref HttpRequestDetails requestDetails)
    {
        RequestContext requestContext = _requestHistory.AddOrUpdate(
            key: GetHistoryItemCacheKey(ref requestDetails),
            addValue: new RequestContext(ref requestDetails),
            updateValueFactory: (_, existingRequestData) => existingRequestData);

        requestContext.IncrementCounter();

        var foundItem = _requestDefinitionProvider.FindItem(ref requestContext);
        return new MockedRequestDefinition(requestContext, foundItem);
    }
}