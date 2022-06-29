using HttpServerMock.RequestDefinitions;
using System.Net.Mime;

namespace HttpServerMock.Server.Models
{
    public struct ResponseDetails : IResponseDetails
    {
        public ResponseDetails(int statusCode, string? content, string contentType, IDictionary<string, string>? headers)
        {
            StatusCode = statusCode;
            Content = content;
            ContentType = contentType;
            Headers = headers;
        }

        public int StatusCode { get; set; }
        public IDictionary<string, string>? Headers { get; set; }
        public string? Content { get; set; }
        public string ContentType { get; set; } = MediaTypeNames.Application.Json;
    }
}