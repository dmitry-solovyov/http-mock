using HttpMock.Models;

namespace HttpMock.RequestProcessing;

public interface ICommandRequestHandler : IRequestHandler
{
    ValueTask Execute(CommandRequestDetails commandRequestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default);
}