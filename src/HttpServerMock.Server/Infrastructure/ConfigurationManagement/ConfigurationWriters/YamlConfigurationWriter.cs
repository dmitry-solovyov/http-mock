using SharpYaml.Serialization;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;

public class YamlConfigurationWriter : IConfigurationWriter
{
    public string ContentType => "application/yaml";

    public string Write(ref ConfigurationBatchDto configurationDefinition)
    {
        var serializer = new Serializer(new SerializerSettings
        {
            IgnoreNulls = true,
            DefaultStyle = SharpYaml.YamlStyle.Block,
            EmitAlias = false,
        });

        var yaml = serializer.Serialize(configurationDefinition);

        return yaml;
    }
}