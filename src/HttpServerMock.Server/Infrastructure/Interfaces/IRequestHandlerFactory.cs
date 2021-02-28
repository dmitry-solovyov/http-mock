using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestHandlerFactory
    {
        Task<RequestHandlerContext> GetHandlerContext(HttpContext httpContext, CancellationToken cancellationToken);
    }
}