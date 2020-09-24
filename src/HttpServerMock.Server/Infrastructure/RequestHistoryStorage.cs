using System.Collections.Concurrent;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHistoryStorage : IRequestHistoryStorage
    {
        private readonly ConcurrentDictionary<string, MockedRequest> _requestHistory = new ConcurrentDictionary<string, MockedRequest>();

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

        public MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails)
        {
            var mockedRequest = _requestHistory.AddOrUpdate(
                MockedRequest.GetKey(requestDetails),
                new MockedRequest(requestDetails),
                (k, existingRequestData) => existingRequestData);

            mockedRequest.Increment();

            var foundItems = _requestDefinitionProvider.FindItems(mockedRequest);
            if (foundItems.Length <= 0)
                return new MockedRequestDefinition(mockedRequest, null);

            var index = mockedRequest.Counter <= 0 ? 0 : mockedRequest.Counter - 1;
            if (index >= foundItems.Length)
            {
                index %= foundItems.Length;
            }

            return new MockedRequestDefinition(mockedRequest, foundItems[index]);
        }
    }
}