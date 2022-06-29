using HttpServerMock.RequestDefinitions;

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
            var contentType = _httpContextAccessor.HttpContext?.Request.ContentType;

            var reader = _requestDefinitionWriters.FirstOrDefault(x => x.IsContentTypeSupported(contentType));
            if (reader != null)
                return reader;

            throw new NotImplementedException($"Content type `{contentType}` is not supported!");
        }
    }
}