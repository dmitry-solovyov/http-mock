using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;

public class JsonConfigurationWriter : IConfigurationWriter
{
    public string ContentType => MediaTypeNames.Application.Json;

    public string Write(ref ConfigurationBatchDto configurationDefinition)
    {
        var settings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyFields = true
        };

        var json = JsonSerializer.Serialize(configurationDefinition, settings);

        return json;
    }
}