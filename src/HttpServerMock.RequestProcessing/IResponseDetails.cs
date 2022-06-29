using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public interface IResponseDetails
    {
        int StatusCode { get; }
        IDictionary<string, string>? Headers { get; }
        string? Content { get; }
        string ContentType { get; }
    }
}
