using HttpServerMock.RequestDefinitions;
using SharpYaml.Serialization;

namespace HttpServerMock.RequestDefinitions.ContentTypeProcessors.Yaml
{
    public class YamlRequestDefinitionWriter : IRequestDefinitionWriter
    {
        public string ContentType => "application/yaml";

        public string Write(ref ConfigurationDefinition configurationDefinition)
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
}