using System.Collections.Generic;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IResponseDetails
    {
        int StatusCode { get; }
        IDictionary<string, string>? Headers { get; }
        string? Content { get; }
        string ContentType { get; }
    }

    public class ResponseDetails : IResponseDetails
    {
        public int StatusCode { get; set; }
        public IDictionary<string, string>? Headers { get; set; }
        public string? Content { get; set; }
        public string ContentType { get; set; } = "application/json";
    }
}
