using SharpYaml.Serialization;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;

public class YamlConfigurationReader : IConfigurationReader
{
    public string ContentType => "application/yaml";

    public ValueTask<ConfigurationBatchDto> Read(Stream contentStream, CancellationToken cancellationToken = default)
    {
        var serializer = new Serializer(new SerializerSettings
        {
            IgnoreUnmatchedProperties = true
        });

        var configuration = serializer.Deserialize<ConfigurationBatchDto>(contentStream);

        return ValueTask.FromResult(configuration);
    }
}