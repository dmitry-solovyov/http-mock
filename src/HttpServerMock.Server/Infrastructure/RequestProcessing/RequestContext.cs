namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public class RequestContext
{
    public RequestContext(ref HttpRequestDetails httpRequestDetails, int counter = 0)
    {
        HttpRequestDetails = httpRequestDetails;
        _counter = counter;
    }

    public HttpRequestDetails HttpRequestDetails { get; }

    private int _counter;

    public int Counter
    {
        get => _counter;
        private set => IncrementCounter(value);
    }

    public RequestContext IncrementCounter(int value = 1)
    {
        Interlocked.Add(ref _counter, value);
        return this;
    }
}