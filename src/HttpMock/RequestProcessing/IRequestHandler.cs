namespace HttpMock.RequestProcessing;

public interface IRequestHandler
{
    ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default);
}