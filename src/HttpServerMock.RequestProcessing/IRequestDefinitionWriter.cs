namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionWriter
    {
        string ContentType { get; }

        bool IsContentTypeSupported(string? contentType) => string.Equals(ContentType, contentType, System.StringComparison.OrdinalIgnoreCase);

        string Write(ref ConfigurationDefinition configurationDefinition);
    }
}