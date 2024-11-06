using HttpMock.Configuration;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class ResetCountersCommandHandler : ICommandRequestHandler
{
    private readonly ILogger<ResetCountersCommandHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;

    public ResetCountersCommandHandler(
        ILogger<ResetCountersCommandHandler> logger,
        IConfigurationStorage configurationStorage)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
    }

    public static string CommandName => "reset-counters";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        if (!CommandValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, errorMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        _logger.LogDebug($"Reset counters for domain: {requestDetails.Domain}");

        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _configurationStorage.ResetUsageCounters(requestDetails.Domain!);

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, cancellationToken).ConfigureAwait(false);
    }
}