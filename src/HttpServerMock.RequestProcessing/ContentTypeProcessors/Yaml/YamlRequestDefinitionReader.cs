using HttpServerMock.RequestDefinitions;
using SharpYaml.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.RequestDefinitions.ContentTypeProcessors.Yaml
{
    public class YamlRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => "application/yaml";

        public Task<ConfigurationDefinition> Read(Stream contentStream, CancellationToken cancellationToken = default)
        {
            var serializer = new Serializer(new SerializerSettings
            {
                IgnoreUnmatchedProperties = true
            });
            var configuration = serializer.Deserialize<ConfigurationDefinition>(contentStream);
            return Task.FromResult(configuration);
        }
    }
}
