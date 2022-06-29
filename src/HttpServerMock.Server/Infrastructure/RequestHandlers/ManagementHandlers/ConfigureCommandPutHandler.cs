using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestDefinitions.Converters;
using HttpServerMock.RequestDefinitions.Extensions;
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
                return ResponseDetailsFactory.Status400BadRequest();
            }

            var requestDefinitionReader = _requestDefinitionReaderProvider.GetReader();

            var configurationDefinition = await requestDefinitionReader.Read(_httpContextAccessor.HttpContext.Request.Body);
            if (!ConfigurationDefinitionExtensions.HasData(ref configurationDefinition))
            {
                return ResponseDetailsFactory.Status404NotFound();
            }

            var requestDefinitions = ConfigurationDefinitionConverter.ToDefinitionSet(ref configurationDefinition);
            if (requestDefinitions == null)
            {
                return ResponseDetailsFactory.Status404NotFound();
            }

            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation($"Setup configuration: {requestDefinitions.DefinitionItems.Count()} items");

            _requestDefinitionStorage.AddSet(requestDefinitions);

            _requestHistoryStorage.Clear();

            return ResponseDetailsFactory.Status202Accepted();
        }
    }
}