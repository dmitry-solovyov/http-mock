using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class UnknownCommandHandler : ICommandRequestHandler
{
    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        await httpResponse
            .WithStatusCode(StatusCodes.Status400BadRequest)
            .WithContentAsync($"Cannot handle command '{requestDetails.CommandName}'", cancellationToken: cancellationToken);
    }
}