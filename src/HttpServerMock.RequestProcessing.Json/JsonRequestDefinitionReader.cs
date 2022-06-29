using HttpServerMock.RequestDefinitions;
using System.IO;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.RequestDefinitionProcessing.Json
{
    public class JsonRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public async Task<ConfigurationDefinition> Read(Stream contentStream, CancellationToken cancellationToken = default)
        {
            var configuration = await JsonSerializer.DeserializeAsync<ConfigurationDefinition>(contentStream, cancellationToken: cancellationToken);
            return configuration;
        }
    }
}