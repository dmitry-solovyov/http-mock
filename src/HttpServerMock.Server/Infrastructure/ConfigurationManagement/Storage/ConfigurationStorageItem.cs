namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public record class ConfigurationStorageItem(string? Description, ConfigurationStorageItemEndpointFilter When, ConfigurationStorageItemResponseDefinition Then)
{
    public Guid Id { get; } = Guid.NewGuid();
}
