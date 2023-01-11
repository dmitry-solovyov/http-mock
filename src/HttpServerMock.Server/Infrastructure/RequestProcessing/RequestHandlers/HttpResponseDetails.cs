using System.Net.Mime;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers;

public record struct HttpResponseDetails
{
    public HttpResponseDetails(int statusCode, string? content, string contentType, IDictionary<string, string>? headers, int delay)
    {
        StatusCode = statusCode;
        Content = content;
        ContentType = contentType;
        Headers = headers;
        Delay = delay;
    }

    public int StatusCode { get; set; }
    public IDictionary<string, string>? Headers { get; set; }
    public string? Content { get; set; }
    public string ContentType { get; set; } = MediaTypeNames.Application.Json;
    public int Delay { get; set; }
}