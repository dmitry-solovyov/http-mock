namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public struct ConfigurationBatchDto
{
    public string? Info { get; set; }

    public List<ConfigurationItemDto>? Map { get; set; }

    public bool HasData => Map?.Count > 0;

    public bool IsEmpty => !HasData;
}
