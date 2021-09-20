using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public interface IRequestDefinitionWriterProvider
    {
        IRequestDefinitionWriter GetWriter();
    }
}
