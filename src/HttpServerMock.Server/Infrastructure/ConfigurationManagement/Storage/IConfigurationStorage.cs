using HttpServerMock.Server.Infrastructure.RequestProcessing;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public interface IConfigurationStorage
{
    int GetCount();

    void Clear();

    void AddSet(ConfigurationStorageItemSet definitionSet);

    ConfigurationStorageItem? FindItem(ref RequestContext request);

    IReadOnlyCollection<ConfigurationStorageItemSet> GetDefinitionSets();
}