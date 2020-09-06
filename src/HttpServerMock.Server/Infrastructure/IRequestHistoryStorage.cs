using HttpServerMock.RequestRuntime;
using HttpServerMock.Server.Models;
using System.Collections.Concurrent;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IRequestHistoryStorage
    {
        void Clear();
        MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails);
    }

    public class RequestHistoryStorage : IRequestHistoryStorage
    {
        private readonly ConcurrentDictionary<string, MockedRequest> _requestHistory = new ConcurrentDictionary<string, MockedRequest>();

        private readonly IRequestDefinitionProvider _requestDefinitionProvider;
        private MockedRequest? _mockedRequest;

        public RequestHistoryStorage(IRequestDefinitionProvider requestDefinitionProvider)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
        }

        public void Clear()
        {
            _requestHistory.Clear();
        }

        public MockedRequestDefinition GetMockedRequestWithDefinition(IRequestDetails requestDetails)
        {
            _mockedRequest ??= _requestHistory.AddOrUpdate(
                MockedRequest.GetKey(requestDetails),
                new MockedRequest(requestDetails),
                (k, existingRequestData) => existingRequestData);

            var foundItems = _requestDefinitionProvider.FindItems(_mockedRequest);
            if (foundItems.Length <= 0)
                return new MockedRequestDefinition(_mockedRequest, null);

            var index = _mockedRequest.Counter <= 0 ? 0 : _mockedRequest.Counter - 1;
            if (index >= foundItems.Length)
            {
                index %= foundItems.Length;
            }

            return new MockedRequestDefinition(_mockedRequest, foundItems[index]);
        }
    }
}