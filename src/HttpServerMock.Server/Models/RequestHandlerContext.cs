using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Models
{
    public class RequestHandlerContext
    {
        public RequestHandlerContext(IRequestDetails requestDetails, IRequestHandler requestHandler)
        {
            RequestDetails = requestDetails;
            RequestHandler = requestHandler;
        }

        public IRequestDetails RequestDetails { get; }

        public IRequestHandler RequestHandler { get; }
    }
}
