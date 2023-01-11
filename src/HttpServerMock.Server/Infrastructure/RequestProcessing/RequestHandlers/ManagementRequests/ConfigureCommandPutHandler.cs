using HttpServerMock.Server.Infrastructure.ConfigurationManagement;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;

public class ConfigureCommandPutHandler : IRequestHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfigurationStorage _requestDefinitionStorage;
    private readonly IConfigurationReaderProvider _requestDefinitionReaderProvider;
    private readonly IRequestHistoryStorage _requestHistoryStorage;
    private readonly ILogger<ConfigureCommandPutHandler> _logger;

    public ConfigureCommandPutHandler(
        IHttpContextAccessor httpContextAccessor,
        IConfigurationStorage requestDefinitionStorage,
        IConfigurationReaderProvider requestDefinitionReaderProvider,
        IRequestHistoryStorage requestHistoryStorage,
        ILogger<ConfigureCommandPutHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _requestDefinitionStorage = requestDefinitionStorage;
        _requestDefinitionReaderProvider = requestDefinitionReaderProvider;
        _requestHistoryStorage = requestHistoryStorage;
        _logger = logger;
    }

    public async ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Set configuration");

        cancellationToken.ThrowIfCancellationRequested();

        if (_httpContextAccessor.HttpContext == null)
        {
            return ResponseDetailsFactory.Status400BadRequest();
        }

        var requestDefinitionReader = _requestDefinitionReaderProvider.GetReader();

        var configurationDefinition = await requestDefinitionReader.Read(_httpContextAccessor.HttpContext.Request.Body);

        var requestDefinitions = ConfigurationDefinitionConverter.ToDefinitionStorageItemSet(ref configurationDefinition);
        if (requestDefinitions == null)
        {
            return ResponseDetailsFactory.Status400BadRequest();
        }

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation($"Setup configuration: {requestDefinitions.DefinitionItems.Count()} items");

        _requestDefinitionStorage.AddSet(requestDefinitions);

        _requestHistoryStorage.Clear();

        return ResponseDetailsFactory.Status202Accepted();
    }
}