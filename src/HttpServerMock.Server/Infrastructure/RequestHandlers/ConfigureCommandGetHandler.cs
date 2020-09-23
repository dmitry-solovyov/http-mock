using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Net.Mime;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ConfigureCommandGetHandler : IRequestDetailsHandler
    {
        private readonly IRequestDefinitionProvider _requestDefinitionProvider;
        private readonly ILogger<ConfigureCommandGetHandler> _logger;

        public ConfigureCommandGetHandler(
            IRequestDefinitionProvider requestDefinitionProvider,
            ILogger<ConfigureCommandGetHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Get;

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            _logger.LogInformation("Set configuration");
            return Task.FromResult(ProcessGetCommand(requestDetails));
        }

        private IResponseDetails? ProcessGetCommand(IRequestDetails requestDetails)
        {
            var contentType = requestDetails.ContentType;
            return (contentType.ToLower() switch
            {
                "application/yaml" => GenerateYamlContent(),
                _ => GenerateJsonContent()
            });
        }

        private IResponseDetails? GenerateYamlContent()
        {
            var serializer = new YamlDotNet.Serialization.Serializer();

            var array = new ArrayList();
            foreach (var item in _requestDefinitionProvider.GetItems())
            {
                array.Add(item);
                array.Add(null);
            }

            var yaml = serializer.Serialize(new { map = array });

            return new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK,
                Content = yaml,
                ContentType = "application/yaml"
            };
        }

        private IResponseDetails? GenerateJsonContent()
        {
            var serializer = new YamlDotNet.Serialization.Serializer();

            var array = new ArrayList();
            foreach (var item in _requestDefinitionProvider.GetItems())
            {
                array.Add(item);
                array.Add(null);
            }

            var json = serializer.Serialize(new { map = array });

            return new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK,
                Content = json,
                ContentType = MediaTypeNames.Application.Json
            };
        }
    }
}
