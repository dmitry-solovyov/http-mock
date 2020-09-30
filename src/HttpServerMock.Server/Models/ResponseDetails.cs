﻿using System.Collections.Generic;
using HttpServerMock.Server.Infrastructure.Interfaces;

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