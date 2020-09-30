using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Models
{
    public class MockedRequestDefinition
    {
        public MockedRequestDefinition(RequestContext mockedRequest, RequestDefinitionItem? requestDefinition)
        {
            MockedRequest = mockedRequest;
            RequestDefinition = requestDefinition;
        }

        public RequestContext MockedRequest { get; }
        public RequestDefinitionItem? RequestDefinition { get; }
    }
}
