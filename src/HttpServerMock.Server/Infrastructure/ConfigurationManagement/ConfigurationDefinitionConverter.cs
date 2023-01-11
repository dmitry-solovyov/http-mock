using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using System.Net;
using System.Net.Mime;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public static class ConfigurationDefinitionConverter
{
    public static ConfigurationStorageItemSet? ToDefinitionStorageItemSet(ref ConfigurationBatchDto configurationDefinition)
    {
        if (configurationDefinition.IsEmpty)
            return null;

        var items = configurationDefinition.Map!.Select(ToDefinitionStorageItem);

        return new ConfigurationStorageItemSet(
            configurationDefinition.Info,
            items.ToArray()
        );
    }

    private static ConfigurationStorageItem ToDefinitionStorageItem(ConfigurationItemDto requestConfigurationDefinition)
    {
        return new ConfigurationStorageItem(
            requestConfigurationDefinition.Description,
            new ConfigurationStorageItemEndpointFilter(requestConfigurationDefinition.Url, true),
            new ConfigurationStorageItemResponseDefinition(
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

    public static ConfigurationBatchDto ToConfigurationDefinition(ConfigurationStorageItemSet requestDefinitionItemSet)
    {
        if (!requestDefinitionItemSet.DefinitionItems.Any())
            return default;

        var items = requestDefinitionItemSet.DefinitionItems!.Select(ToConfigurationDefinitionItem);

        return new ConfigurationBatchDto
        {
            Info = requestDefinitionItemSet.DefinitionName,
            Map = items.ToList()
        };
    }

    private static ConfigurationItemDto ToConfigurationDefinitionItem(ConfigurationStorageItem requestDefinitionItem)
    {
        return new ConfigurationItemDto
        {
            Url = requestDefinitionItem.When.Url,
            Description = requestDefinitionItem.Description,
            Headers = requestDefinitionItem.Then.Headers?.ToDictionary(x => x.Key, x => x.Value),
            Payload = requestDefinitionItem.Then.Payload,
            Method = requestDefinitionItem.Then.Method,
            Status = requestDefinitionItem.Then.StatusCode,
            Delay = requestDefinitionItem.Then.Delay,
        };
    }
}