namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public struct ConfigurationItemDto
{
    public string? Url { get; set; }

    public string? Description { get; set; }

    public string? Method { get; set; }

    public int? Status { get; set; }

    public int? Delay { get; set; }

    public string? Payload { get; set; }

    public Dictionary<string, string>? Headers { get; set; }
}
