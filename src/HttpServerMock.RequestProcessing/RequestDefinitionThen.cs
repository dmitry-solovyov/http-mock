using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionThen
    {
        public string ContentType { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Payload { get; set; }
        public int StatusCode { get; set; }
        public int? Delay { get; set; }
        public string? ProxyUrl { get; set; } = null;
        public Dictionary<string,string>? Headers { get; set; }
    }
}
