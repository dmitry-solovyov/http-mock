using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ResetConfigurationCommandHandler : IRequestHandler
    {
        private readonly IRequestDefinitionStorage _requestDefinitionProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetConfigurationCommandHandler> _logger;

        public ResetConfigurationCommandHandler(
            IRequestDefinitionStorage requestDefinitionProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetConfigurationCommandHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public ValueTask<Models.ResponseDetails> Execute(RequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Reset configuration. Current number of definition items {_requestDefinitionProvider.GetCount()}");

            _requestDefinitionProvider.Clear();
            _requestHistoryStorage.Clear();

            return ValueTask.FromResult(ResponseDetailsFactory.Status200OK());
        }
    }
}