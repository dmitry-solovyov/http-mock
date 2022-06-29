using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class RequestDefinitionReaderProvider : IRequestDefinitionReaderProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IRequestDefinitionReader> _requestDefinitionReaders;

        public RequestDefinitionReaderProvider(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IRequestDefinitionReader> requestDefinitionReaders)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDefinitionReaders = requestDefinitionReaders;
        }

        public IRequestDefinitionReader GetReader()
        {
            var contentType = _httpContextAccessor.HttpContext?.Request.ContentType;

            var reader = _requestDefinitionReaders.FirstOrDefault(x => x.IsContentTypeSupported(contentType));
            if (reader != null)
                return reader;

            throw new NotImplementedException($"Content type `{contentType}` is not supported!");
        }
    }
}