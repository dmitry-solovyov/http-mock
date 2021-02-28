using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ConfigureCommandGetHandler : IRequestHandler
    {
        private readonly IRequestDefinitionStorage _requestDefinitionProvider;
        private readonly ILogger<ConfigureCommandGetHandler> _logger;

        public ConfigureCommandGetHandler(
            IRequestDefinitionStorage requestDefinitionProvider,
            ILogger<ConfigureCommandGetHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _logger = logger;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Set configuration");

            var contentType = requestDetails.ContentType;
            var content = contentType.ToLower() switch
            {
                "application/yaml" => GenerateYamlContent(),
                _ => GenerateJsonContent()
            };

            return Task.FromResult(content);
        }

        private IResponseDetails GenerateYamlContent()
        {
            var serializer = new YamlDotNet.Serialization.Serializer();

            var array = new ArrayList();
            foreach (var item in _requestDefinitionProvider.GetDefinitionSets())
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

        private IResponseDetails GenerateJsonContent()
        {
            var serializer = new YamlDotNet.Serialization.Serializer();

            var array = new ArrayList();
            foreach (var item in _requestDefinitionProvider.GetDefinitionSets())
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
