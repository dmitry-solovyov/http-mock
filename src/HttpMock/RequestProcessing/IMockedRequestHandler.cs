using HttpMock.Models;

namespace HttpMock.RequestProcessing;

public interface IMockedRequestHandler : IRequestHandler
{
    ValueTask Execute(MockedRequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default);
}