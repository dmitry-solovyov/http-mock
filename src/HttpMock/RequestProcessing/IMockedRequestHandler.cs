using HttpMock.Models;

namespace HttpMock.RequestProcessing;

public interface IMockedRequestHandler : IRequestHandler
{
    ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default);
}