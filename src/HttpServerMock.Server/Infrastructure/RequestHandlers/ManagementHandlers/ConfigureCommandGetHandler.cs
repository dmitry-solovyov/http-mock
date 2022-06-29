using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestDefinitions.Converters;
using HttpServerMock.Server.Infrastructure.Interfaces;
using System.Net;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ConfigureCommandGetHandler : IRequestHandler
    {
        private readonly ILogger<ConfigureCommandGetHandler> _logger;
        private readonly IRequestDefinitionStorage _requestDefinitionProvider;
        private readonly IRequestDefinitionWriterProvider _requestDefinitionWriteProvider;

        public ConfigureCommandGetHandler(
            ILogger<ConfigureCommandGetHandler> logger,
            IRequestDefinitionStorage requestDefinitionProvider,
            IRequestDefinitionWriterProvider requestDefinitionWriteProvider)
        {
            _logger = logger;
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestDefinitionWriteProvider = requestDefinitionWriteProvider;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            return Task.FromResult(ProcessGetCommand(requestDetails, cancellationToken));
        }

        private IResponseDetails ProcessGetCommand(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Get configuration");

            var requestDefinitionWriter = _requestDefinitionWriteProvider.GetWriter();

            var definitionSets = _requestDefinitionProvider.GetDefinitionSets();
            var fisrstDefinition = definitionSets.FirstOrDefault();
            if (fisrstDefinition == null)
            {
                return PreDefinedResponses.Status404NotFound.Value;
            }

            var configurationDefinition = ConfigurationDefinitionConverter.ToConfigurationDefinition(fisrstDefinition);
            if (!configurationDefinition.HasData)
            {
                return PreDefinedResponses.Status400BadRequest.Value;
            }

            var content = requestDefinitionWriter.Write(ref configurationDefinition);
            if (content == null)
            {
                return PreDefinedResponses.Status400BadRequest.Value;
            }

            return new Models.ResponseDetails { StatusCode = StatusCodes.Status200OK, Content = content };
        }
    }
}