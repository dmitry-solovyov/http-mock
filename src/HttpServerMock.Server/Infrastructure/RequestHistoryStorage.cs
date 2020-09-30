using System.Collections.Concurrent;
using System.Linq;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHistoryStorage : IRequestHistoryStorage
    {
        private readonly ConcurrentDictionary<string, RequestContext> _requestHistory = new ConcurrentDictionary<string, RequestContext>();

        private readonly IRequestDefinitionProvider _requestDefinitionProvider;

        public RequestHistoryStorage(IRequestDefinitionProvider requestDefinitionProvider)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
        }

        public int CurrentItemsCount => _requestHistory.Count;

        public void Clear()
        {
            _requestHistory.Clear();
        }

        public static string GetKey(IRequestDetails requestDetails) => $"{requestDetails.HttpMethod}_{requestDetails.Uri}";

        public MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails)
        {
            var requestContext = _requestHistory.AddOrUpdate(
                GetKey(requestDetails),
                new RequestContext(requestDetails),
                (k, existingRequestData) => existingRequestData);

            requestContext.IncrementCounter();

            var foundItems = _requestDefinitionProvider.FindItems(requestContext);
            if (!foundItems.Any())
                return new MockedRequestDefinition(requestContext, null);

            var index = requestContext.Counter <= 0 ? 0 : requestContext.Counter - 1;
            if (index >= foundItems.Length)
            {
                index %= foundItems.Length;
            }

            return new MockedRequestDefinition(requestContext, foundItems[index]);
        }
    }
}