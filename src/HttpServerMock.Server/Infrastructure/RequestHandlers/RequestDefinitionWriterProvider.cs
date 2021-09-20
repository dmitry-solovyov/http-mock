using HttpServerMock.RequestDefinitions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class RequestDefinitionWriterProvider : IRequestDefinitionWriterProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IRequestDefinitionWriter> _requestDefinitionWriters;

        public RequestDefinitionWriterProvider(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IRequestDefinitionWriter> requestDefinitionWriters)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDefinitionWriters = requestDefinitionWriters;
        }

        public IRequestDefinitionWriter GetWriter()
        {
            var contentType = _httpContextAccessor.HttpContext.Request.ContentType;

            var reader = _requestDefinitionWriters.FirstOrDefault(x => CompareContentType(x.ContentType, contentType));
            if (reader != null)
                return reader;

            throw new NotImplementedException($"Content type `{contentType}` is not supported!");
        }
        private static bool CompareContentType(string contentType1, string contentType2) =>
            string.Equals(contentType1, contentType2, StringComparison.OrdinalIgnoreCase);
    }
}