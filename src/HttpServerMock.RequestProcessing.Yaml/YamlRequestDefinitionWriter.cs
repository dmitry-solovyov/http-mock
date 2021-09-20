using HttpServerMock.RequestDefinitions;
using System.Collections;

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
            var serializer = new YamlDotNet.Serialization.Serializer();

            var array = new ArrayList();
            foreach (var item in _requestDefinitionProvider.GetDefinitionSets(string.Empty)) //TODO: place proper department
            {
                array.Add(item);
                array.Add(null);
            }

            var yaml = serializer.Serialize(new { map = array });
            return yaml;
        }
    }
}
