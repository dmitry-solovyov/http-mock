namespace HttpServerMock.Server.Infrastructure.RequestProcessing
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

            var requestRouter = httpContext.RequestServices.GetRequiredService<IRequestRouter>();

            if (!requestRouter.TryGetHandlerContext(httpContext, out var handlerContext))
            {
                httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
                await httpContext.Response.CompleteAsync();
                return;
            }

            var responseDetails = await handlerContext.RequestHandler.Execute(handlerContext.RequestDetails, cancellationToken)
                .ConfigureAwait(false);

            httpContext.Response.StatusCode = responseDetails.StatusCode;
            httpContext.Response.ContentType = responseDetails.ContentType;

            if (responseDetails.Headers?.Any() == true)
            {
                foreach (var header in responseDetails.Headers)
                    httpContext.Response.Headers.Add(header.Key, header.Value);
            }

            if (!string.IsNullOrWhiteSpace(responseDetails.Content))
            {
                var data = System.Text.Encoding.UTF8.GetBytes(responseDetails.Content);
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