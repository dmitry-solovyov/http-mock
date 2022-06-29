using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace HttpServerMock.Server.Middleware
{
    public class RequestLoggerMiddleware
    {
        private static readonly int LogFirstNCharsFromHeader = -1;

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            LogStartOfRequest(httpContext);

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _logger.LogInformation($"[Request cancelled] {LogUrl(httpContext.Request)}[thread={Thread.CurrentThread.ManagedThreadId}]");
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

        private void LogStartOfRequest(HttpContext httpContext)
        {
            var logMessage = new StringBuilder($"[Start request] {LogUrl(httpContext.Request)}[thread={Thread.CurrentThread.ManagedThreadId}]");
            if (httpContext.Request.Headers.Count > 0)
            {
                logMessage.AppendLine();
                logMessage.AppendLine("Headers:");
                foreach (var headerKey in httpContext.Request.Headers.Keys)
                {
                    logMessage.AppendLine($"    {headerKey}={GetFirstChars(httpContext.Request.Headers[headerKey])}");
                }
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private static string GetFirstChars(string text)
        {
            if (string.IsNullOrEmpty(text) || LogFirstNCharsFromHeader <= 0 || text.Length < LogFirstNCharsFromHeader)
                return text;

            return $"{text.Substring(0, LogFirstNCharsFromHeader)}...";
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