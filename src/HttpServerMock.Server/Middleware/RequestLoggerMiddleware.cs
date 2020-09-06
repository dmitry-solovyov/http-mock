using System;
using System.Threading;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace HttpServerMock.Server.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails();

            MockedRequest? mockedRequest = null;
            if (!requestDetails.IsCommandRequest(out _))
            {
                var requestHistoryContainer = httpContext.RequestServices.GetService<IRequestHistoryStorage>();
                mockedRequest = requestHistoryContainer.GetMockedRequestWithDefinition(requestDetails).MockedRequest.Increment();
            }

            Console.WriteLine();
            ConsoleExtensions.Write($"[IN] {LogEntryHeader(httpContext.Request, mockedRequest)}", ConsoleColor.Cyan);

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                ConsoleExtensions.Write($"{DateTimeOffset.UtcNow:O} EXCEPTION {ex.Message}\n{ex.StackTrace}", ConsoleColor.Red);
                throw;
            }
            finally
            {
                ConsoleExtensions.Write($"[OUT] {LogEntryHeader(httpContext.Request, mockedRequest)} -> {httpContext.Response?.StatusCode} ", ConsoleColor.DarkGreen);
            }
        }

        private static string LogEntryHeader(HttpRequest request, MockedRequest? requestData)
        {
            return $"{DateTimeOffset.UtcNow:O} {request.Method} {request.GetDisplayUrl()}  #{requestData?.Counter ?? 0} [{Thread.CurrentThread.ManagedThreadId}]";
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