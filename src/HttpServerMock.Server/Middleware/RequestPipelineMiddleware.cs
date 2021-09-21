using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Middleware
{
    public class RequestPipelineMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestPipelineMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext? httpContext)
        {
            if (httpContext == null)
            {
                await _next(httpContext);
                return;
            }

            var cancellationToken = httpContext.RequestAborted;

            var requestHandlerFactory = httpContext.RequestServices.GetService<IRequestHandlerFactory>();

            var handlerContext = await requestHandlerFactory.GetHandlerContext(httpContext, cancellationToken).ConfigureAwait(false);

            var responseDetails = await handlerContext.RequestHandler.Execute(handlerContext.RequestDetails, cancellationToken).ConfigureAwait(false);

            httpContext.Response.StatusCode = responseDetails.StatusCode;
            httpContext.Response.ContentType = responseDetails.ContentType;

            if (!string.IsNullOrWhiteSpace(responseDetails.Content))
            {
                var data = Encoding.UTF8.GetBytes(responseDetails.Content);
                await httpContext.Response.Body.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            }

            await httpContext.Response.CompleteAsync();
        }
    }

    public static class RequestHandlerPipelineMiddlewareExtensions
    {
        public static void UseRequestPipeline(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestPipelineMiddleware>();
        }
    }
}
