namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionWriter
    {
        string ContentType { get; }
        string Write(RequestDefinitionItemSet requestDefinitionSet);
    }
}
