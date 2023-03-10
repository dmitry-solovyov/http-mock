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
                requestConfigurationDefinition.Headers,
                ToDefinitionStorageProxyItem(requestConfigurationDefinition.Proxy),
                ToDefinitionStorageCallbackItem(requestConfigurationDefinition.Callback)
            )
        );
    }

    private static ConfigurationStorageItemCallback? ToDefinitionStorageCallbackItem(ConfigurationItemCallbackDto? callback)
    {
        if (callback == null)
            return null;

        return new ConfigurationStorageItemCallback(
            callback.Value.Url,
            callback.Value.Async
        );
    }

    private static ConfigurationStorageItemProxy? ToDefinitionStorageProxyItem(ConfigurationItemProxyDto? proxy)
    {
        if (proxy == null)
            return null;

        return new ConfigurationStorageItemProxy(
            proxy.Value.Url,
            proxy.Value.Async
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
            Proxy = ToConfigurationProxyDefinitionItem(requestDefinitionItem.Then.Proxy),
            Callback = ToConfigurationCallbackDefinitionItem(requestDefinitionItem.Then.Callback),
        };
    }

    private static ConfigurationItemProxyDto? ToConfigurationProxyDefinitionItem(ConfigurationStorageItemProxy? proxy)
    {
        if (proxy == null)
            return null;

        return new ConfigurationItemProxyDto(
            proxy.Url,
            proxy.Async
        );
    }

    private static ConfigurationItemCallbackDto? ToConfigurationCallbackDefinitionItem(ConfigurationStorageItemCallback? callback)
    {
        if (callback == null)
            return null;

        return new ConfigurationItemCallbackDto(
            callback.Url,
            callback.Async
        );
    }
}