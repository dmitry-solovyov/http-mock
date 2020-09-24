using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.RequestHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Middleware
{
    public class RequestHandlerPipelineMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestHandlerPipelineMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private static readonly IEnumerable<Type> HandlersQueue =
            new[]
            {
                typeof(ResetStatisticsCommandHandler),
                typeof(ConfigureCommandGetHandler),
                typeof(ConfigureCommandPutHandler),
                typeof(MockedRequestHandler)
            };

        public async Task Invoke(HttpContext httpContext)
        {
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails().ConfigureAwait(false);

            var services = httpContext.RequestServices.GetServices<IRequestDetailsHandler>().ToArray();

            foreach (var handlerType in HandlersQueue)
            {
                var handler = services.FirstOrDefault(x => x.GetType() == handlerType);
                if (handler == null || !handler.CanHandle(requestDetails))
                    continue;

                var result = await handler.HandleResponse(requestDetails);
                if (result == null)
                    continue;

                httpContext.Response.StatusCode = result.StatusCode;
                httpContext.Response.ContentType = result.ContentType;
                if (!string.IsNullOrWhiteSpace(result.Content))
                {
                    var data = Encoding.UTF8.GetBytes(result.Content);
                    await httpContext.Response.Body.WriteAsync(data, CancellationToken.None).ConfigureAwait(false);
                }
                await httpContext.Response.CompleteAsync();
                return;
            }

            await _next(httpContext);
        }
    }

    public static class RequestHandlerPipelineMiddlewareExtensions
    {
        public static void UseRequestHandlerQueue(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestHandlerPipelineMiddleware>();
        }
    }
}
