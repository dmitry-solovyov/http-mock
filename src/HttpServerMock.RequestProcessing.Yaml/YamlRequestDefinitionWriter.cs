using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.RequestDefinitionProcessing.Yaml
{
    public class YamlRequestDefinitionWriter : IRequestDefinitionWriter
    {
        private readonly IRequestDefinitionStorage _requestDefinitionProvider;

        public YamlRequestDefinitionWriter(IRequestDefinitionStorage requestDefinitionProvider)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
        }

        public string ContentType => "application/yaml";

        public string Write(RequestDefinitionItemSet requestDefinitionSet)
        {
            var serializer = new SharpYaml.Serialization.Serializer();

            var yaml = serializer.Serialize(new { });

            return yaml;
        }
    }
}