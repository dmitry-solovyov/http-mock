using HttpMock.Models;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HttpMock.Serializations;

public class YamlSerialization : ISerialization
{
    public string SupportedContentType => "application/yaml";

    #region Serialization

    private const int MaximumRecursion = 10;

    public string Serialize(DomainConfigurationDto model)
    {
        var serializer = BuildSerializer();

        var yaml = serializer.Serialize(model);

        return yaml;
    }

    public ValueTask SerializeAsync(DomainConfigurationDto model, Stream contentStream, CancellationToken cancellationToken = default)
    {
        var serializer = BuildSerializer();

        var yaml = serializer.Serialize(model);

        return contentStream.WriteAsync(Encoding.UTF8.GetBytes(yaml), cancellationToken);
    }

    private static ISerializer BuildSerializer()
    {
        var aotContext = new YamlDotNet.Static.StaticContext();

        var builder = new StaticSerializerBuilder(aotContext)
            .WithMaximumRecursion(MaximumRecursion)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections)
            .WithEnumNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNamingConvention(PascalCaseNamingConvention.Instance);

        return builder.Build();
    }

    #endregion Serialization

    #region Deserialization 

    public DomainConfigurationDto? Deserialize(string content)
    {
        var deserializer = BuildDeserialize();
        var result = deserializer.Deserialize<DomainConfigurationDto>(content);
        return result;
    }

    public async ValueTask<DomainConfigurationDto?> DeserializeAsync(Stream contentStream, CancellationToken cancellationToken = default)
    {
        var deserializer = BuildDeserialize();

        TextReader textReader = new StreamReader(contentStream);
        var yaml = await textReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        var result = deserializer.Deserialize<DomainConfigurationDto>(yaml);
        return result;
    }

    private static IDeserializer BuildDeserialize()
    {
        var aotContext = new YamlDotNet.Static.StaticContext();

        var builder = new StaticDeserializerBuilder(aotContext)
            .WithEnumNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties();

        return builder.Build();
    }

    #endregion Deserialization
}
