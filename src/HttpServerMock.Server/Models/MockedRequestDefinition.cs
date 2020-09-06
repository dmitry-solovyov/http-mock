using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Models
{
    public class MockedRequestDefinition
    {
        public MockedRequestDefinition(MockedRequest mockedRequest, RequestDefinition? requestDefinition)
        {
            MockedRequest = mockedRequest;
            RequestDefinition = requestDefinition;
        }

        public MockedRequest MockedRequest { get; }
        public RequestDefinition? RequestDefinition { get; }
    }
}
