using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public class ConfigurationReaderProvider : IConfigurationReaderProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<IConfigurationReader> _configurationReaders;

    public ConfigurationReaderProvider(
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<IConfigurationReader> configurationReaders)
    {
        _httpContextAccessor = httpContextAccessor;
        _configurationReaders = configurationReaders;
    }

    public IConfigurationReader GetReader()
    {
        var contentType = _httpContextAccessor.HttpContext?.Request.ContentType;

        var reader = _configurationReaders.FirstOrDefault(x => x.IsContentTypeSupported(contentType));
        if (reader != null)
            return reader;

        throw new NotImplementedException($"Content type `{contentType}` is not supported!");
    }
}