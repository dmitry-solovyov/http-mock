using HttpServerMock.Server.Infrastructure.Interfaces;
using System.Collections.Generic;

namespace HttpServerMock.Server.Models
{
    public class ResponseDetails : IResponseDetails
    {
        public int StatusCode { get; set; }
        public IDictionary<string, string>? Headers { get; set; }
        public string? Content { get; set; }
        public string ContentType { get; set; } = "application/json";
    }
}