using HttpMock.Configuration;
using HttpMock.RequestProcessing;
using System.Text;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class GetStatisticsCommandHandler : ICommandRequestHandler
{
    private readonly ILogger<ResetCountersCommandHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;

    public GetStatisticsCommandHandler(
        ILogger<ResetCountersCommandHandler> logger,
        IConfigurationStorage configurationStorage)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
    }

    public static string CommandName => "get-counters";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        if (!CommandValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status400BadRequest, errorMessage, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var requiredLength = 0;
        foreach (var endpointConfiguration in domainConfiguration!.Endpoints)
            requiredLength += endpointConfiguration.When.Url.Length + 10;

        var sb = new StringBuilder(requiredLength);
        foreach (var endpointConfiguration in domainConfiguration.Endpoints)
        {
            sb.AppendLine($"{endpointConfiguration.When.Url} - {endpointConfiguration.CallCounter}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, sb.ToString(), cancellationToken).ConfigureAwait(false);
    }
}