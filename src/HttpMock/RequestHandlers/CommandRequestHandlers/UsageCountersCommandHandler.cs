using HttpMock.Configuration;
using HttpMock.RequestProcessing;
using System.Text;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class UsageCountersCommandHandler(IConfigurationStorage configurationStorage) : ICommandRequestHandler
{
    public const string CommandName = "usage-counters";

    private readonly IConfigurationStorage _configurationStorage = configurationStorage;

    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!RequestValidationRules.IsDomainValid(ref requestDetails, out var errorMessage))
        {
            await httpResponse
                .WithStatusCode(StatusCodes.Status400BadRequest)
                .WithContentAsync(errorMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        var handleResult = requestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(requestDetails, httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(requestDetails, httpResponse, cancellationToken),
            _ => Unknown(requestDetails, httpResponse, cancellationToken),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var nl = Environment.NewLine.Length;

        var requiredLength = 0;
        foreach (var endpointConfiguration in domainConfiguration!.Endpoints)
            requiredLength += endpointConfiguration.When.Url.Length + 3 + nl +
                endpointConfiguration.CallCounter switch
                {
                    <= 9 => 1,
                    <= 99 => 2,
                    <= 999 => 3,
                    _ => 4
                };

        var sb = new StringBuilder(requiredLength);
        foreach (var endpointConfiguration in domainConfiguration.Endpoints)
        {
            sb.AppendLine($"{endpointConfiguration.When.Url} - {endpointConfiguration.CallCounter}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse
            .WithStatusCode(StatusCodes.Status200OK)
            .WithContentAsync(sb.ToString(), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private ValueTask Delete(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain!, out var domainConfiguration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return ValueTask.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _configurationStorage.ResetUsageCounters(requestDetails.Domain!);

        httpResponse.WithStatusCode(StatusCodes.Status200OK);
        return ValueTask.CompletedTask;
    }

    private async ValueTask Unknown(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        await httpResponse
            .WithStatusCode(StatusCodes.Status405MethodNotAllowed)
            .WithContentAsync($"Cannot handle command '{CommandName}'", cancellationToken: cancellationToken);
    }
}