namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        string ContentType { get; }

        bool IsContentTypeSupported(string? contentType) => string.Equals(ContentType, contentType, System.StringComparison.OrdinalIgnoreCase);

        ConfigurationDefinition Read(string requestContent);
    }
}