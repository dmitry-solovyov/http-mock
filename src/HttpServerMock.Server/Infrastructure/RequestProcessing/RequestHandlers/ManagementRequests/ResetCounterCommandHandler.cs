using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;

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

    public ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Reset counters. Current number of history items {_requestHistoryStorage.CurrentItemsCount}");

        _requestHistoryStorage.Clear();

        return ValueTask.FromResult(ResponseDetailsFactory.Status200OK());
    }
}