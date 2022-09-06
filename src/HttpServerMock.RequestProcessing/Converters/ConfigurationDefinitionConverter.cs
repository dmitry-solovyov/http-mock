using System.Linq;
using System.Net;
using System.Net.Mime;

namespace HttpServerMock.RequestDefinitions.Converters;

public static class ConfigurationDefinitionConverter
{
    public static RequestDefinitionItemSet? ToDefinitionSet(ref ConfigurationDefinition configurationDefinition)
    {
        if (!configurationDefinition.HasData())
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
            new ResponseDefinition(
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

    public static ConfigurationDefinition ToConfigurationDefinition(RequestDefinitionItemSet requestDefinitionItemSet)
    {
        if (!requestDefinitionItemSet.DefinitionItems.Any())
            return default;

        var items = requestDefinitionItemSet.DefinitionItems!.Select(ToConfigurationDefinitionItem);

        return new ConfigurationDefinition
        {
            Info = requestDefinitionItemSet.DefinitionName,
            Map = items.ToList()
        };
    }

    private static RequestConfigurationDefinition ToConfigurationDefinitionItem(RequestDefinitionItem requestDefinitionItem)
    {
        return new RequestConfigurationDefinition
        {
            Url = requestDefinitionItem.When.Url,
            Description = requestDefinitionItem.Description,
            //Headers = requestDefinitionItem.Then.Headers,
            Payload = requestDefinitionItem.Then.Payload,
            Method = requestDefinitionItem.Then.Method,
            Status = requestDefinitionItem.Then.StatusCode,
            Delay = requestDefinitionItem.Then.Delay,
        };
    }
}