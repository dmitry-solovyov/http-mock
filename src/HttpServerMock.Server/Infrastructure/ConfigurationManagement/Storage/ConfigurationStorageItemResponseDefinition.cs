namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public record class ConfigurationStorageItemResponseDefinition(
    string ContentType,
    string? Method,
    string? Payload,
    int StatusCode,
    int? Delay,
    IReadOnlyDictionary<string, string>? Headers,
    ConfigurationStorageItemProxy? Proxy,
    ConfigurationStorageItemCallback? Callback
)
{
    public bool HasProxy => string.IsNullOrEmpty(Proxy?.Url) is not true;
    public bool HasCallback => string.IsNullOrEmpty(Callback?.Url) is not true;
}

public record class ConfigurationStorageItemCallback(
    string? Url,
    bool? Async
);

public record class ConfigurationStorageItemProxy(
    string? Url,
    bool? Async
);