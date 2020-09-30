using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Middleware
{
    public class RequestPipelineMiddleware<TRequestHandler> : IMiddleware
        where TRequestHandler : IRequestHandler
    {
        private readonly TRequestHandler _request;

        public RequestPipelineMiddleware(TRequestHandler request)
        {
            _request = request;
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails().ConfigureAwait(false);

            if (_request.CanHandle(requestDetails))
            {
                var result = await _request.HandleResponse(requestDetails);
                if (result != null)
                {
                    httpContext.Response.StatusCode = result.StatusCode;
                    httpContext.Response.ContentType = result.ContentType;
                    if (!string.IsNullOrWhiteSpace(result.Content))
                    {
                        var data = Encoding.UTF8.GetBytes(result.Content);
                        await httpContext.Response.Body.WriteAsync(data, CancellationToken.None).ConfigureAwait(false);
                    }

                    await httpContext.Response.CompleteAsync();

                    return;
                }
            }

            await next(httpContext);
        }
    }

    public static class RequestHandlerPipelineMiddlewareExtensions
    {
        public static void UseRequestHandlerQueue<TRequestHandler>(this IApplicationBuilder builder)
            where TRequestHandler : IRequestHandler
        {
            builder.UseMiddleware<RequestPipelineMiddleware<TRequestHandler>>();
        }
    }
}
