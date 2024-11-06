using HttpMock.Models;

namespace HttpMock.Serializations
{
    public interface ISerialization
    {
        string SupportedContentType { get; }

        DomainConfigurationDto? Deserialize(string content);
        ValueTask<DomainConfigurationDto?> DeserializeAsync(Stream contentStream, CancellationToken cancellationToken = default);
        string Serialize(DomainConfigurationDto model);
        ValueTask SerializeAsync(DomainConfigurationDto model, Stream contentStream, CancellationToken cancellationToken = default);
    }
}