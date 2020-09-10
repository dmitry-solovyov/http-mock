using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Middleware
{
    public class MockedRequestHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public MockedRequestHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails().ConfigureAwait(false);

            if (!requestDetails.IsCommandRequest(out _))
            {
                var requestHistoryStorage = httpContext.RequestServices.GetService<IRequestHistoryStorage>();
                var mockedRequestWithDefinition = requestHistoryStorage.GetMockedRequestWithDefinition(requestDetails);

                if (await ProcessRequestDefinition(httpContext, mockedRequestWithDefinition, CancellationToken.None))
                    return;
            }

            await _next(httpContext);
        }

        private static async ValueTask<bool> ProcessRequestDefinition(HttpContext context, MockedRequestDefinition mockedRequestWithDefinition, CancellationToken cancellationToken)
        {
            var requestDefinition = mockedRequestWithDefinition.RequestDefinition;
            if (requestDefinition == null)
                return false;

            var handled = false;

            handled |= FillContentType(context, requestDefinition);

            handled |= FillStatusCode(context, requestDefinition);

            handled |= await FillDelay(requestDefinition, cancellationToken);

            handled |= await FillPayload(context, requestDefinition, cancellationToken);

            handled |= FillHeaders(context, requestDefinition);

            return handled;
        }

        private static bool FillContentType(HttpContext context, RequestDefinition requestDefinition)
        {
            if (!string.IsNullOrWhiteSpace(requestDefinition.Then.ContentType))
            {
                context.Response.ContentType = requestDefinition.Then.ContentType;
                return true;
            }

            context.Response.ContentType = MediaTypeNames.Application.Json;
            return false;
        }

        private static bool FillStatusCode(
            HttpContext context, RequestDefinition requestDefinition)
        {
            if (requestDefinition.Then.StatusCode <= 0)
                return false;

            context.Response.StatusCode = requestDefinition.Then.StatusCode;
            return true;
        }

        private static async ValueTask<bool> FillDelay(
            RequestDefinition requestDefinition, CancellationToken cancellationToken)
        {
            if (!requestDefinition.Then.Delay.HasValue || requestDefinition.Then.Delay.Value <= 0)
                return false;

            await Task.Delay(requestDefinition.Then.Delay.Value, cancellationToken).ConfigureAwait(false);
            return true;
        }

        private static async ValueTask<bool> FillPayload(
            HttpContext context, RequestDefinition requestDefinition, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestDefinition.Then.Payload))
                return false;

            var encoding = Encoding.UTF8;

            var payload = requestDefinition.Then.Payload;
            if (!string.IsNullOrWhiteSpace(payload))
            {
                while (payload.Contains("@guid"))
                    payload = payload.Replace("@guid", Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);

                foreach (var urlVariable in requestDefinition.When.UrlVariables)
                    while (payload.Contains($"@{urlVariable}"))
                    {
                        var match = Regex.Match(context.Request.GetDisplayUrl(), requestDefinition.When.Url, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        payload = payload.Replace($"@{urlVariable}", match.Groups.ContainsKey(urlVariable)
                            ? match.Groups[urlVariable].Value
                            : Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);
                    }
            }

            var data = encoding.GetBytes(payload);
            context.Response.ContentType = requestDefinition.Then.ContentType;
            context.Response.ContentLength = data.Length;
            await context.Response.Body.WriteAsync(data, cancellationToken).ConfigureAwait(false);

            return true;
        }

        private static bool FillHeaders(HttpContext context, RequestDefinition requestDefinition)
        {
            if (requestDefinition.Then.Headers == null || requestDefinition.Then.Headers.Count == 0)
                return false;

            foreach (var thenHeader in requestDefinition.Then.Headers)
            {
                context.Response.Headers[thenHeader.Key] = thenHeader.Value;
            }
            return true;
        }
    }
    public static class MockedRequestHandlerMiddlewareExtensions
    {
        public static void UseMockedRequestHandler(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<MockedRequestHandlerMiddleware>();
        }
    }
}
