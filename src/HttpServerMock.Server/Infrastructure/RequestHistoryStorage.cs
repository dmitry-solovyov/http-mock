using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHistoryStorage : IRequestHistoryStorage
    {
        private readonly ConcurrentDictionary<string, RequestContext> _requestHistory = new ConcurrentDictionary<string, RequestContext>();

        private readonly IRequestDefinitionStorage _requestDefinitionProvider;

        public RequestHistoryStorage(IRequestDefinitionStorage requestDefinitionProvider)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
        }

        public int CurrentItemsCount => _requestHistory.Count;

        public void Clear() => _requestHistory.Clear();

        public static string GetHistoryItemCacheKey(IRequestDetails requestDetails) => $"{requestDetails.HttpMethod}_{requestDetails.Uri}";

        public MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            var requestContext = _requestHistory.AddOrUpdate(
                GetHistoryItemCacheKey(requestDetails),
                new RequestContext(requestDetails),
                (k, existingRequestData) => existingRequestData);

            requestContext.IncrementCounter();

            cancellationToken.ThrowIfCancellationRequested();

            var foundItems = _requestDefinitionProvider.FindItems(requestContext, cancellationToken).ToArray();
            if (!foundItems.Any())
                return new MockedRequestDefinition(requestContext, null);

            var index = requestContext.Counter <= 0 ? 0 : requestContext.Counter - 1;
            if (index >= foundItems.Length)
                index %= foundItems.Length;

            return new MockedRequestDefinition(requestContext, foundItems[index]);
        }
    }
}