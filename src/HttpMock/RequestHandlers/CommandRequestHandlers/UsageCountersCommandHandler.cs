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
            HttpMethodType.Get => Get(httpResponse, cancellationToken),
            HttpMethodType.Delete => Delete(httpResponse),
            _ => Unknown(httpResponse),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (!_configurationStorage.TryGetConfiguration(out var configuration))
        {
            httpResponse.WithStatusCode(StatusCodes.Status404NotFound);
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var content = GetContentForUsageStatistics(configuration);

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedReadCommands)
            .WithContentAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private ValueTask Delete(HttpResponse httpResponse)
    {
        _configurationStorage.ResetUsageCounters();

        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedUpdateCommands);

        return ValueTask.CompletedTask;
    }

    private static ValueTask Unknown(HttpResponse httpResponse)
    {
        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownCommandMethodType);
        return ValueTask.CompletedTask;
    }

    private static string GetContentForUsageStatistics(Models.Configuration configuration)
    {
        var requiredOutputStringLength = CalculateRequiredOutputStringLength(configuration);

        var sb = new StringBuilder(requiredOutputStringLength);
        foreach (var endpointConfiguration in configuration.Endpoints)
        {
            sb.Append(endpointConfiguration.When.Path);
            sb.Append(" - ");
            sb.Append(endpointConfiguration.CallCounter);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static int CalculateRequiredOutputStringLength(Models.Configuration configuration)
    {
        var requiredLength = 0;
        foreach (var endpointConfiguration in configuration!.Endpoints)
            requiredLength += endpointConfiguration.When.Path.Length + 5;

        return requiredLength;
    }
}