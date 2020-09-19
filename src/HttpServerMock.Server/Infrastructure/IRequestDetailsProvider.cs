using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IRequestDetailsProvider
    {
        Task<IRequestDetails> GetRequestDetails();
    }

    public class RequestDetailsProvider : IRequestDetailsProvider
    {
        private readonly IHttpContextAccessor _accessor;

        private IRequestDetails? _requestDetails;

        public RequestDetailsProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task<IRequestDetails> GetRequestDetails()
        {
            if (_requestDetails != null)
                return _requestDetails;

            var httpContext = _accessor.HttpContext;
            var request = httpContext.Request;

            string? requestContent = null;
            if ((request.ContentLength ?? 0) > 0)
                requestContent = await request.BodyReader.ReadPipeAsync();

            var result = new RequestDetails(
                request.Method,
                request.GetDisplayUrl(),
                request.Headers.ToDictionary(x => x.Key, x => x.Value.Select(s => s).ToArray()),
                httpContext.Connection.RemoteIpAddress.ToString(),
                requestContent,
                request.ContentType
            );

            _requestDetails = result;

            return result;
        }
    }
}