using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement
{
    public class ConfigurationWriterProvider : IConfigurationWriterProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IConfigurationWriter> _configurationWriters;

        public ConfigurationWriterProvider(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IConfigurationWriter> configurationWriters)
        {
            _httpContextAccessor = httpContextAccessor;
            _configurationWriters = configurationWriters;
        }

        public IConfigurationWriter GetWriter()
        {
            var contentType = _httpContextAccessor.HttpContext?.Request.ContentType;

            var reader = _configurationWriters.FirstOrDefault(x => x.IsContentTypeSupported(contentType));
            if (reader != null)
                return reader;

            throw new NotImplementedException($"Content type `{contentType}` is not supported!");
        }
    }
}