using HttpServerMock.Server.Infrastructure.Interfaces;
using System.Text;

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
                return;

            var cancellationToken = httpContext.RequestAborted;

            var requestHandlerFactory = httpContext.RequestServices.GetRequiredService<IRequestHandlerRouter>();

            if (!requestHandlerFactory.TryGetHandler(httpContext, out var handlerContext))
            {
                await httpContext.Response.CompleteAsync();
                return;
            }

            var responseDetails = await handlerContext.RequestHandler.Execute(handlerContext.RequestDetails, cancellationToken)
                .ConfigureAwait(false);

            httpContext.Response.StatusCode = responseDetails.StatusCode;
            httpContext.Response.ContentType = responseDetails.ContentType;

            if (responseDetails.Headers?.Count > 0)
            {
                foreach (var header in responseDetails.Headers)
                    httpContext.Response.Headers.Add(header.Key, header.Value);
            }

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