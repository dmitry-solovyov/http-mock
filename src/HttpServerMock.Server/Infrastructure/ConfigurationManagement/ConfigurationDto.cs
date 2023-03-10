namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public record struct ConfigurationBatchDto(string? Info, IList<ConfigurationItemDto>? Map)
{
    public bool HasData => Map?.Count > 0;
    public bool IsEmpty => !HasData;
}

public record struct ConfigurationItemDto(
    string? Url,
    string? Description,
    string? ContentType,
    string? Method,
    int? Status,
    int? Delay,
    string? Payload,
    Dictionary<string, string>? Headers,
    ConfigurationItemProxyDto? Proxy,
    ConfigurationItemCallbackDto? Callback
);

public record struct ConfigurationItemCallbackDto(
    string? Url,
    bool? Async
);

public record struct ConfigurationItemProxyDto(
    string? Url,
    bool? Async
);
