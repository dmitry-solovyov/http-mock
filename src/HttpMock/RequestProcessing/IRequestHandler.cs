namespace HttpMock.RequestProcessing;

public interface IRequestHandler
{
    ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default);
}