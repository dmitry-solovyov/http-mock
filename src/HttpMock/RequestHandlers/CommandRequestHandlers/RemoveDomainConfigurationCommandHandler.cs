using HttpMock.Configuration;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class RemoveDomainConfigurationCommandHandler : ICommandRequestHandler
{
    private readonly ILogger<RemoveDomainConfigurationCommandHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;

    public RemoveDomainConfigurationCommandHandler(
        ILogger<RemoveDomainConfigurationCommandHandler> logger,
        IConfigurationStorage configurationStorage)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
    }

    public static string CommandName => "remove-configuration";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        if (!CommandValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, errorMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogDebug($"Remove configuration for domain: {requestDetails.Domain}");

        if (!_configurationStorage.TryRemoveDomain(requestDetails.Domain!))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, $"Domain removed: {requestDetails.Domain}", cancellationToken).ConfigureAwait(false);
    }
}