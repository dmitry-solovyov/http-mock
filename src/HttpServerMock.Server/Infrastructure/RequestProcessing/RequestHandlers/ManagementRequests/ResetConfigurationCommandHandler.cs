using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;

public class ResetConfigurationCommandHandler : IRequestHandler
{
    private readonly IConfigurationStorage _requestDefinitionProvider;
    private readonly IRequestHistoryStorage _requestHistoryStorage;
    private readonly ILogger<ResetConfigurationCommandHandler> _logger;

    public ResetConfigurationCommandHandler(
        IConfigurationStorage requestDefinitionProvider,
        IRequestHistoryStorage requestHistoryStorage,
        ILogger<ResetConfigurationCommandHandler> logger)
    {
        _requestDefinitionProvider = requestDefinitionProvider;
        _requestHistoryStorage = requestHistoryStorage;
        _logger = logger;
    }

    public ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Reset configuration. Current number of definition items {_requestDefinitionProvider.GetCount()}");

        _requestDefinitionProvider.Clear();
        _requestHistoryStorage.Clear();

        return ValueTask.FromResult(ResponseDetailsFactory.Status200OK());
    }
}