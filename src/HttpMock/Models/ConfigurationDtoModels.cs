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
    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? ContentType { get; set; }
    public ushort? Status { get; set; }
    public ushort Delay { get; set; } = 0;
    public string? Payload { get; set; }
    public Dictionary<string, string?>? Headers { get; set; }
}