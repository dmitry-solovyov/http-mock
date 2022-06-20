using HttpServerMock.RequestDefinitions;
using SharpYaml.Serialization;

namespace HttpServerMock.RequestDefinitionProcessing.Yaml
{
    public class YamlRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => "application/yaml";

        public ConfigurationDefinition Read(string requestContent)
        {
            var serializer = new Serializer();
            var configuration = serializer.Deserialize<ConfigurationDefinition>(requestContent);
            return configuration;
        }
    }
}
