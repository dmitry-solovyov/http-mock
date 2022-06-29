using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestDefinitions.Converters;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ConfigureCommandPutHandler : IRequestHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestDefinitionStorage _requestDefinitionStorage;
        private readonly IRequestDefinitionReaderProvider _requestDefinitionReaderProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ConfigureCommandPutHandler> _logger;

        public ConfigureCommandPutHandler(
            IHttpContextAccessor httpContextAccessor,
            IRequestDefinitionStorage requestDefinitionStorage,
            IRequestDefinitionReaderProvider requestDefinitionReaderProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ConfigureCommandPutHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDefinitionStorage = requestDefinitionStorage;
            _requestDefinitionReaderProvider = requestDefinitionReaderProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public async Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Set configuration");

            cancellationToken.ThrowIfCancellationRequested();

            if (_httpContextAccessor.HttpContext == null)
            {
                return PreDefinedResponses.Status400BadRequest.Value;
            }

            var requestDefinitionReader = _requestDefinitionReaderProvider.GetReader();

            var configurationDefinitions = await requestDefinitionReader.Read(_httpContextAccessor.HttpContext.Request.Body);
            if (!configurationDefinitions.HasData)
            {
                return PreDefinedResponses.Status404NotFound.Value;
            }

            var requestDefinitions = ConfigurationDefinitionConverter.ToDefinitionSet(ref configurationDefinitions);
            if (requestDefinitions == null)
            {
                return PreDefinedResponses.Status404NotFound.Value;
            }

            _logger.LogInformation($"Setup configuration: {requestDefinitions.DefinitionItems.Count()} items");

            _requestDefinitionStorage.AddSet(requestDefinitions);

            _requestHistoryStorage.Clear();

            return PreDefinedResponses.Status202Accepted.Value;
        }
    }
}