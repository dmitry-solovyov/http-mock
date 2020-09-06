using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
                //httpContext.Request.EnableBuffering();

                await _next(httpContext);
            }
            catch
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await httpContext.Response.WriteAsync("API failed on request processing!", CancellationToken.None);
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
