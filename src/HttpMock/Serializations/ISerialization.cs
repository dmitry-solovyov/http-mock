using HttpMock.Models;

namespace HttpMock.Serializations
{
    public interface ISerialization
    {
        string SupportedContentType { get; }

        ConfigurationDto? Deserialize(string content);
        ValueTask<ConfigurationDto?> DeserializeAsync(Stream contentStream, CancellationToken cancellationToken = default);
        string Serialize(ConfigurationDto model);
        ValueTask SerializeAsync(ConfigurationDto model, Stream contentStream, CancellationToken cancellationToken = default);
    }
}