﻿using HttpServerMock.Server.Infrastructure.Interfaces;
using System.Collections.Generic;

namespace HttpServerMock.Server.Models
{
    public class RequestDetails : IRequestDetails
    {
        public RequestDetails(string httpMethod, string uri, IReadOnlyDictionary<string, string[]> headers, string clientAddress, string? content, string contentType)
        {
            HttpMethod = httpMethod;
            Uri = uri;
            Headers = headers;
            ClientAddress = clientAddress;
            ContentType = contentType;
            Content = content;
        }

        public string HttpMethod { get; }
        public string Uri { get; }
        public IReadOnlyDictionary<string, string[]>? Headers { get; }
        public string ContentType { get; }
        public string? Content { get; }
        public string ClientAddress { get; }
    }
}