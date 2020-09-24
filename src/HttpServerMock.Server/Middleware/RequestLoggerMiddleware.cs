using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure.Interfaces;

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
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails();

            //MockedRequest? mockedRequest = null;
            if (!requestDetails.IsCommandRequest(out _))
            {
                //var requestHistoryContainer = httpContext.RequestServices.GetService<IRequestHistoryStorage>();
                //mockedRequest = requestHistoryContainer.GetMockedRequestWithDefinition(requestDetails).MockedRequest.Increment();
            }

            _logger.LogInformation($"[Start request] {LogUrl(httpContext.Request)}[thread={Thread.CurrentThread.ManagedThreadId}]");

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"EXCEPTION {ex.Message}");
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