namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;

public interface IConfigurationReader
{
    string ContentType { get; }

    bool IsContentTypeSupported(string? contentType) => string.Equals(ContentType, contentType, StringComparison.OrdinalIgnoreCase);

    ValueTask<ConfigurationBatchDto> Read(Stream contentStream, CancellationToken cancellationToken = default);
}