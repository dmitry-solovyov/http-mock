using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.RequestDefinitions.ContentTypeProcessors.Json
{
    public class JsonRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public async Task<ConfigurationDefinition> Read(Stream contentStream, CancellationToken cancellationToken = default)
        {
            var settings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IgnoreReadOnlyFields = true
            };

            var configuration = await JsonSerializer.DeserializeAsync<ConfigurationDefinition>(contentStream, settings, cancellationToken);

            return configuration;
        }
    }
}