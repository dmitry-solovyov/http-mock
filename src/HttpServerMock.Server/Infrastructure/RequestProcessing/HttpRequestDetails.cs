namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public struct HttpRequestDetails
{
    private IReadOnlyDictionary<string, string>? _headers;

    public HttpRequestDetails(string httpMethod, string uri, IReadOnlyDictionary<string, string>? headers, string? clientAddress, string contentType)
    {
        HttpMethod = httpMethod;
        Url = uri;
        ClientAddress = clientAddress;
        ContentType = contentType;
        _headers = headers;
    }

    public string HttpMethod { get; }
    public string Url { get; }
    public string ContentType { get; }
    public string? ClientAddress { get; }

    public string? GetHeaderValue(string headerName)
    {
        if (_headers?.TryGetValue(headerName, out var foundHeader) == true)
            return foundHeader;

        return default;
    }
}