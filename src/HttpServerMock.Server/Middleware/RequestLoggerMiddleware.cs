using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _logger.LogInformation($"[Start request] {LogUrl(httpContext.Request)}[thread={Thread.CurrentThread.ManagedThreadId}]");

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EXCEPTION {ex.Message}");
                _logger.LogDebug(ex.StackTrace);
                throw;
            }
            finally
            {
                _logger.LogInformation($"[Finish request] {LogUrl(httpContext.Request)}[thread={Thread.CurrentThread.ManagedThreadId}, response={httpContext.Response?.StatusCode}]");
            }
        }

        private static string LogUrl(HttpRequest request)
        {
            return $"({request.Method}) {request.GetDisplayUrl()} ";
        }
    }

    public static class RequestLoggerMiddlewareExtensions
    {
        public static void UseRequestLogger(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestLoggerMiddleware>();
        }
    }
}