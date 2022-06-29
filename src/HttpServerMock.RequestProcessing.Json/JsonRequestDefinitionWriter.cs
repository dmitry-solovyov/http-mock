using HttpServerMock.RequestDefinitions;
using System.Net.Mime;

namespace HttpServerMock.RequestDefinitionProcessing.Json
{
    public class JsonRequestDefinitionWriter : IRequestDefinitionWriter
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public string Write(ref ConfigurationDefinition configurationDefinition)
        {
            var content = System.Text.Json.JsonSerializer.Serialize(configurationDefinition);
            return content;
        }
    }
}
