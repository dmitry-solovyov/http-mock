namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public record class ConfigurationStorageItemSet(string? DefinitionName, IEnumerable<ConfigurationStorageItem> DefinitionItems)
{
    public Guid Id { get; } = Guid.NewGuid();
}