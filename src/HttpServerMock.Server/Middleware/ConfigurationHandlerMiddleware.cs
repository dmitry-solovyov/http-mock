using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Middleware
{
    public class ConfigurationHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ConfigurationHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestDetailsProvider = httpContext.RequestServices.GetService<IRequestDetailsProvider>();
            var requestDetails = await requestDetailsProvider.GetRequestDetails().ConfigureAwait(false);

            if (requestDetails.IsCommandRequest(out var commandName))
            {
                if (Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("| Set configuration");

                    if (await ProcessPutCommand(httpContext).ConfigureAwait(false) ||
                        await ProcessGetCommand(httpContext).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                else if (Constants.HeaderValues.ResetStatisticsCommandName.Equals(commandName,
                  StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("| Reset statistics");

                    var requestHistoryContainer = httpContext.RequestServices.GetService<IRequestHistoryStorage>();
                    requestHistoryContainer.Clear();

                    return;
                }
            }

            await _next(httpContext);
        }

        private static async ValueTask<bool> ProcessGetCommand(HttpContext httpContext)
        {
            if (httpContext.Request.Method != HttpMethods.Get)
                return false;

            var contentType = httpContext.Request.ContentType;
            await (contentType.ToLower() switch
            {
                "application/yaml" => GenerateYamlContent(httpContext),
                _ => GenerateJsonContent(httpContext)
            });
            return true;
        }

        private static async ValueTask<bool> GenerateYamlContent(HttpContext httpContext)
        {
            var encoding = Encoding.UTF8;
            var serializer = new YamlDotNet.Serialization.Serializer();

            var requestDefinitionProvider = httpContext.RequestServices.GetService<IRequestDefinitionProvider>();

            var array = new ArrayList();
            foreach (var item in requestDefinitionProvider.GetItems())
            {
                array.Add(item);
                array.Add(null);
            }

            var yaml = serializer.Serialize(new { map = array });
            var data = encoding.GetBytes(yaml);

            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            await httpContext.Response.Body.WriteAsync(data, CancellationToken.None).ConfigureAwait(false);
            await httpContext.Response.CompleteAsync();
            return true;
        }

        private static async ValueTask<bool> GenerateJsonContent(HttpContext httpContext)
        {
            var encoding = Encoding.UTF8;
            var serializer = new YamlDotNet.Serialization.Serializer();

            var requestDefinitionProvider = httpContext.RequestServices.GetService<IRequestDefinitionProvider>();

            var array = new ArrayList();
            foreach (var item in requestDefinitionProvider.GetItems())
            {
                array.Add(item);
                array.Add(null);
            }

            var yaml = serializer.Serialize(new { map = array });
            var data = encoding.GetBytes(yaml);

            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            await httpContext.Response.Body.WriteAsync(data, CancellationToken.None).ConfigureAwait(false);
            await httpContext.Response.CompleteAsync();
            return true;
        }

        private async ValueTask<bool> ProcessPutCommand(HttpContext httpContext)
        {
            if (httpContext.Request.Method != HttpMethods.Put)
                return false;

            var requestContent = await httpContext.Request.BodyReader.ReadPipeAsync();

            var requestDefinitionReader = httpContext.RequestServices.GetService<IRequestDefinitionReader>();
            var requestDefinitions = requestDefinitionReader.Read(new StringReader(requestContent));

            var requestDefinitionProvider = httpContext.RequestServices.GetService<IRequestDefinitionProvider>();
            requestDefinitionProvider.AddRange(requestDefinitions);

            httpContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
            await httpContext.Response.CompleteAsync();
            return true;
        }
    }

    public static class YamlConfigurationHandlerMiddlewareExtensions
    {
        public static void UseConfigurationHandler(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ConfigurationHandlerMiddleware>();
        }
    }
}
