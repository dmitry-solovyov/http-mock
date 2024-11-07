using HttpMock.Configuration;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class DomainsCommandHandler : ICommandRequestHandler
{
    private readonly IConfigurationStorage _configurationStorage;

    public DomainsCommandHandler(IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
    }

    public const string CommandName = "domains";

    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var handleResult = requestDetails.HttpMethod switch
        {
            HttpMethodType.Get => Get(requestDetails, httpResponse, cancellationToken),
            _ => Unknown(requestDetails, httpResponse, cancellationToken),
        };

        await handleResult.ConfigureAwait(false);
    }

    private async ValueTask Get(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        var content = string.Join(Environment.NewLine, _configurationStorage.GetDomains());

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse
            .WithStatusCode(StatusCodes.Status200OK)
            .WithContentAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask Unknown(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        await httpResponse
            .WithStatusCode(StatusCodes.Status405MethodNotAllowed)
            .WithContentAsync($"Cannot handle command '{CommandName}'", cancellationToken: cancellationToken);
    }
}