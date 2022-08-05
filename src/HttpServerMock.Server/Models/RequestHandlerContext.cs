using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Models
{
    public readonly struct RequestHandlerContext
    {
        public RequestHandlerContext(RequestDetails requestDetails, IRequestHandler requestHandler)
        {
            RequestDetails = requestDetails;
            RequestHandler = requestHandler;
        }

        public RequestDetails RequestDetails { get; }

        public IRequestHandler RequestHandler { get; }
    }
}
