using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerMock.RequestDefinitions.ContentTypeProcessors.Json
{
    public class JsonRequestDefinitionWriter : IRequestDefinitionWriter
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public string Write(ref ConfigurationDefinition configurationDefinition)
        {
            var settings = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IgnoreReadOnlyFields = true
            };
            var content = JsonSerializer.Serialize(configurationDefinition, settings);
            return content;
        }
    }
}
