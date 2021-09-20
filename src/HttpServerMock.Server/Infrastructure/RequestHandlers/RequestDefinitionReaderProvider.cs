using HttpServerMock.RequestDefinitions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class RequestDefinitionReaderProvider : IRequestDefinitionReaderProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IRequestDefinitionReader> _requestDefinitionReaders;

        public RequestDefinitionReaderProvider(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IRequestDefinitionReader> requestDefinitionReaders
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDefinitionReaders = requestDefinitionReaders;
        }

        public IRequestDefinitionReader GetReader()
        {
            var contentType = _httpContextAccessor.HttpContext.Request.ContentType;

            var reader = _requestDefinitionReaders.FirstOrDefault(x => CompareContentType(x.ContentType, contentType));
            if (reader != null)
                return reader;

            throw new NotImplementedException($"Content type `{contentType}` is not supported!");
        }

        private static bool CompareContentType(string contentType1, string contentType2) =>
            string.Equals(contentType1, contentType2, StringComparison.OrdinalIgnoreCase);
    }
}