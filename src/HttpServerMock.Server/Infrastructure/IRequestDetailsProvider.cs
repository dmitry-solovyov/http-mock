using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IRequestDetailsProvider
    {
        ValueTask<IRequestDetails> GetRequestDetails();
    }

    public class RequestDetailsProvider : IRequestDetailsProvider
    {
        private readonly IHttpContextAccessor _accessor;

        public RequestDetailsProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private RequestDetails? _requestDetails;

        public ValueTask<IRequestDetails> GetRequestDetails()
        {
            if (_requestDetails != null)
                return new ValueTask<IRequestDetails>(_requestDetails);

            var httpContext = _accessor.HttpContext;
            var request = httpContext.Request;

            var result = new RequestDetails(
                request.Method,
                request.GetDisplayUrl(),
                request.Headers.ToDictionary(x => x.Key, x => x.Value.Select(s => s).ToArray()),
                httpContext.Connection.RemoteIpAddress.ToString()
            );
            _requestDetails = result;

            return new ValueTask<IRequestDetails>(result);
        }
    }
}