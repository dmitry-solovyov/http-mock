using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using System.Collections.Concurrent;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHistoryStorage : IRequestHistoryStorage
    {
        private readonly ConcurrentDictionary<string, RequestContext> _requestHistory =
            new ConcurrentDictionary<string, RequestContext>();

        private readonly IRequestDefinitionStorage _requestDefinitionProvider;

        public RequestHistoryStorage(IRequestDefinitionStorage requestDefinitionProvider)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
        }

        public int CurrentItemsCount => _requestHistory.Count;

        public void Clear() => _requestHistory.Clear();

        private static string GetHistoryItemCacheKey(ref RequestDetails requestDetails) => $"{requestDetails.HttpMethod?.ToLower()}_{requestDetails.Url?.ToLower()}";

        public MockedRequestDefinition GetMockedRequestWithDefinition(ref RequestDetails requestDetails)
        {
            var requestContext = _requestHistory.AddOrUpdate(
                key: GetHistoryItemCacheKey(ref requestDetails),
                addValue: new RequestContext(ref requestDetails),
                updateValueFactory: (_, existingRequestData) => existingRequestData);

            requestContext.IncrementCounter();

            var foundItem = _requestDefinitionProvider.FindItem(ref requestContext);
            return new MockedRequestDefinition(requestContext, foundItem);
        }
    }
}