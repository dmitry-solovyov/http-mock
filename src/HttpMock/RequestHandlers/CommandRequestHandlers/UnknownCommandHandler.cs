using HttpMock.Models;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.CommandRequestHandlers;

public class UnknownCommandHandler : IUnknownCommandHandler
{
    public async ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        await httpResponse
            .WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownCommand)
            .WithContentAsync("Command name is unknown!", cancellationToken: cancellationToken);
    }
}