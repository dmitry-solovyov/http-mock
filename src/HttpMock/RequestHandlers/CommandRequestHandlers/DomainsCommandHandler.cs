using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class DomainsCommandHandler : ICommandRequestHandler
{
    private readonly IConfigurationStorage _configurationStorage;

    public DomainsCommandHandler(IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
    }

    public const string CommandName = "domains";

    public async ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var handleResult = commandRequestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(httpResponse, cancellationToken),
            _ => Unknown(httpResponse),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendJoin(Environment.NewLine, _configurationStorage.GetDomains());
        var content = sb.ToString();

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForProcessedReadCommands)
            .WithContentAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static ValueTask Unknown(HttpResponse httpResponse)
    {
        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledMethod);
        return ValueTask.CompletedTask;
    }
}