using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Models
{
    public readonly struct RequestHandlerContext
    {
        public RequestHandlerContext(IRequestDetails? requestDetails, IRequestHandler requestHandler)
        {
            RequestDetails = requestDetails;
            RequestHandler = requestHandler;
        }

        public IRequestDetails? RequestDetails { get; }

        public IRequestHandler RequestHandler { get; }
    }
}
