using HttpMock.Configuration;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class GetDomainsCommandHandler : ICommandRequestHandler
{
    private readonly IConfigurationStorage _configurationStorage;

    public GetDomainsCommandHandler(IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
    }

    public static string CommandName => "get-domains";

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        var content = string.Join(Environment.NewLine, _configurationStorage.GetDomains());

        cancellationToken.ThrowIfCancellationRequested();

        await httpResponse.FillContentAsync(StatusCodes.Status200OK, content, cancellationToken).ConfigureAwait(false);
    }
}