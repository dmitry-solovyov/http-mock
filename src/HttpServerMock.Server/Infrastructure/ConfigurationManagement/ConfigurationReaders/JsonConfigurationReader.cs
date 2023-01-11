using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;

public class JsonConfigurationReader : IConfigurationReader
{
    public string ContentType => MediaTypeNames.Application.Json;

    public async ValueTask<ConfigurationBatchDto> Read(Stream contentStream, CancellationToken cancellationToken = default)
    {
        var settings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyFields = true
        };

        var configuration = await JsonSerializer.DeserializeAsync<ConfigurationBatchDto>(contentStream, settings, cancellationToken);

        return configuration;
    }
}