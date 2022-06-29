using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDetails
    {
        string HttpMethod { get; }
        string Uri { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
        string? ContentType { get; }
        string? ClientAddress { get; }
    }
}
