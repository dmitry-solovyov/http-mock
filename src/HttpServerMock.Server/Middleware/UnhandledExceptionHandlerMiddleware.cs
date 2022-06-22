using System.Net.Mime;

namespace HttpServerMock.Server.Middleware
{
    public class UnhandledExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public UnhandledExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                if (!httpContext.Response.HasStarted)
                {
                    await httpContext.Response.WriteAsync("API failed on request processing!", CancellationToken.None);
                }
            }
        }
    }

    public static class HttpExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseUnhandledExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UnhandledExceptionHandlerMiddleware>();
        }
    }
}