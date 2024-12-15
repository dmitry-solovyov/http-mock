using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class UsageCountersCommandHandler(IConfigurationStorage configurationStorage) : ICommandRequestHandler
{
    public const string CommandName = "usage-counters";

    private readonly IConfigurationStorage _configurationStorage = configurationStorage;

    public async ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var handleResult = commandRequestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(commandRequestDetails, httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(commandRequestDetails, httpResponse, cancellationToken),
            _ => Unknown(httpResponse),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(commandRequestDetails.Domain, out var domainConfiguration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var requiredOutputStringLength = CalculateRequiredOutputStringLength(domainConfiguration);

        var sb = new StringBuilder(requiredOutputStringLength);
        foreach (var endpointConfiguration in domainConfiguration.Endpoints)
        {
            sb.Append(endpointConfiguration.When.Path);
            sb.Append(" - ");
            sb.Append(endpointConfiguration.CallCounter);
            sb.AppendLine();
        }
        var content = sb.ToString();

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedReadCommands)
            .WithContentAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private ValueTask Delete(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(commandRequestDetails.Domain, out var domainConfiguration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return ValueTask.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _configurationStorage.ResetUsageCounters(domainConfiguration.Domain);

        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands);
        return ValueTask.CompletedTask;
    }

    private static ValueTask Unknown(HttpResponse httpResponse)
    {
        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledMethod);
        return ValueTask.CompletedTask;
    }

    private static int CalculateRequiredOutputStringLength(DomainConfiguration domainConfiguration)
    {
        var requiredLength = 0;
        foreach (var endpointConfiguration in domainConfiguration!.Endpoints)
            requiredLength += endpointConfiguration.When.Path.Length + 5;

        return requiredLength;
    }
}