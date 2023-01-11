namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;

public interface IConfigurationWriter
{
    string ContentType { get; }

    bool IsContentTypeSupported(string? contentType) => string.Equals(ContentType, contentType, StringComparison.OrdinalIgnoreCase);

    string Write(ref ConfigurationBatchDto configurationDefinition);
}