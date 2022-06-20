using System.Linq;
using System.Net;
using System.Net.Mime;

namespace HttpServerMock.RequestDefinitions.Converters;

public static class ConfigurationDefinitionConverter
{
    public static RequestDefinitionItemSet? ToDefinitionSet(ConfigurationDefinition configurationDefinition)
    {
        if (!configurationDefinition.HasData)
            return null;

        var items = configurationDefinition.Map!.Select(ToRequestDefinition);

        return new RequestDefinitionItemSet(
            configurationDefinition.Info,
            items.ToArray()
        );
    }

    private static RequestDefinitionItem ToRequestDefinition(RequestConfigurationDefinition requestConfigurationDefinition)
    {
        return new RequestDefinitionItem(
            requestConfigurationDefinition.Description,
            new RequestCondition(requestConfigurationDefinition.Url, false),
            new ResponseDetails(
                MediaTypeNames.Application.Json,
                requestConfigurationDefinition.Method,
                requestConfigurationDefinition.Payload,
                requestConfigurationDefinition.Status ?? (int)HttpStatusCode.OK,
                requestConfigurationDefinition.Delay,
                null,
                requestConfigurationDefinition.Headers
            )
        );
    }
}