using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Models
{
    public class MockedRequest
    {
        public MockedRequest(IRequestDetails requestDetails, int counter = 0)
        {
            RequestDetails = requestDetails;
            Counter = counter;
            RequestDefinition = null;
        }

        public MockedRequest(MockedRequest mockedRequest, RequestDefinition requestDefinition)
        {
            RequestDetails = mockedRequest.RequestDetails;
            Counter = mockedRequest.Counter;
            RequestDefinition = requestDefinition;
        }

        public IRequestDetails RequestDetails { get; }
        public RequestDefinition? RequestDefinition { get; }

        public int Counter { get; private set; }

        public MockedRequest Increment()
        {
            Counter++;
            return this;
        }

        public static string GetKey(IRequestDetails requestDetails) => $"{requestDetails.HttpMethod}_{requestDetails.Uri}";
    }
}
