using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Models
{
    public class RequestContext
    {
        public RequestContext(IRequestDetails requestDetails, int counter = 0)
        {
            RequestDetails = requestDetails;
            Counter = counter;
            RequestDefinition = null;
        }

        public RequestContext(RequestContext mockedRequest, RequestDefinitionItem requestDefinition)
        {
            RequestDetails = mockedRequest.RequestDetails;
            Counter = mockedRequest.Counter;
            RequestDefinition = requestDefinition;
        }

        public IRequestDetails RequestDetails { get; }
        public RequestDefinitionItem? RequestDefinition { get; }

        public int Counter { get; private set; }

        public RequestContext IncrementCounter()
        {
            Counter++;
            return this;
        }
    }
}
