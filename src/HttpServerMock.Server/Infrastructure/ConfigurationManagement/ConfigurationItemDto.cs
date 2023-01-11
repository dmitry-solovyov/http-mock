namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public record struct ConfigurationItemDto(
    string? Url,
    string? Description,
    string? Method,
    int? Status,
    int? Delay,
    string? Payload,
    Dictionary<string, string>? Headers,
    string? ProxyUrl);
