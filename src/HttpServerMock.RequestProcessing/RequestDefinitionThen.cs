using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionThen
    {
        public RequestDefinitionThen(
            string contentType,
            string? method,
            string? payload,
            int statusCode,
            int? delay,
            string? proxyUrl,
            IReadOnlyDictionary<string, string>? headers)
        {
            ContentType = contentType;
            Method = method;
            Payload = payload;
            StatusCode = statusCode;
            Delay = delay;
            ProxyUrl = proxyUrl;
            Headers = headers;
        }

        public string ContentType { get; }
        public string? Method { get; }
        public string? Payload { get; }
        public int StatusCode { get; }
        public int? Delay { get; }
        public string? ProxyUrl { get; }
        public IReadOnlyDictionary<string, string>? Headers { get; }
    }
}
