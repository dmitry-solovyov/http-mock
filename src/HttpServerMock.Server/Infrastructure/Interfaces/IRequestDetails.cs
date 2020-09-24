using System.Collections.Generic;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDetails
    {
        string HttpMethod { get; }
        string Uri { get; }
        IReadOnlyDictionary<string, string[]>? Headers { get; }
        string ContentType { get; }
        string? Content { get; }
        string ClientAddress { get; }
    }
}
