using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ResetCounterCommandHandler : IRequestHandler
    {
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetCounterCommandHandler> _logger;

        public ResetCounterCommandHandler(
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetCounterCommandHandler> logger)
        {
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Reset counters. Current number of history items {_requestHistoryStorage.CurrentItemsCount}");

            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails)new Models.ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}