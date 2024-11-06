using HttpMock.Models;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpMock.Serializations;

[JsonSourceGenerationOptions(
    JsonSerializerDefaults.Web,
    AllowTrailingCommas = true, DefaultBufferSize = 10,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    IgnoreReadOnlyFields = true)]
[JsonSerializable(typeof(DomainConfigurationDto))]
public partial class DomainConfigurationDtoContext : JsonSerializerContext { }

public class JsonSerialization : ISerialization
{
    public string SupportedContentType => MediaTypeNames.Application.Json;

    #region Serialization

    public string Serialize(DomainConfigurationDto model)
    {
        var json = JsonSerializer.Serialize(model, typeof(DomainConfigurationDto), DomainConfigurationDtoContext.Default);
        return json;
    }

    public async ValueTask SerializeAsync(DomainConfigurationDto model, Stream contentStream, CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(contentStream, model, typeof(DomainConfigurationDto), DomainConfigurationDtoContext.Default, cancellationToken);
    }

    #endregion Serialization

    #region Deserialization

    public DomainConfigurationDto? Deserialize(string content)
    {
        var deserializedObject = JsonSerializer.Deserialize(content, typeof(DomainConfigurationDto), DomainConfigurationDtoContext.Default);
        return (DomainConfigurationDto?)deserializedObject;
    }

    public async ValueTask<DomainConfigurationDto?> DeserializeAsync(Stream contentStream, CancellationToken cancellationToken = default)
    {
        var deserializedObject = await JsonSerializer.DeserializeAsync(contentStream, typeof(DomainConfigurationDto), DomainConfigurationDtoContext.Default, cancellationToken);
        return (DomainConfigurationDto?)deserializedObject;
    }

    #endregion Deserialization
}