using YamlDotNet.Serialization;

namespace HttpMock.Models;

[YamlSerializable]
public class DomainConfigurationDto
{
    public EndpointConfigurationDto[]? Endpoints { get; set; }
}

[YamlSerializable]
public class EndpointConfigurationDto
{
    public string? Url { get; set; }
    public string? Description { get; set; }
    public string? Method { get; set; }
    public string? ContentType { get; set; }
    public int? Status { get; set; }
    public int? Delay { get; set; }
    public string? Payload { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? ProxyUrl { get; set; }
    public string? CallbackUrl { get; set; }
}
