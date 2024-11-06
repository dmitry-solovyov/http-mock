namespace HttpMock.Models;

public sealed record class DomainConfiguration(string Domain, IReadOnlyCollection<EndpointConfiguration> Endpoints);

public sealed record class EndpointConfiguration(EndpointRequestConfiguration When, EndpointResponseConfiguration Then, string? Description = default)
{
    public int CallCounter { get; private set; } = default;

    public void ResetCounter() { CallCounter = default; }
    public void IncreaseCounter() { CallCounter++; }
}

public sealed record class EndpointRequestConfiguration(HttpMethodType HttpMethod, string Url, string? UrlRegexExpression = default, IReadOnlyCollection<string>? UrlVariables = default, bool CaseSensitive = false);

public sealed record class EndpointResponseConfiguration(int StatusCode, string ContentType, string? Payload = default, int Delay = default, IReadOnlyDictionary<string, string>? Headers = default, string? ProxyUrl = default, string? CallbackUrl = default);

