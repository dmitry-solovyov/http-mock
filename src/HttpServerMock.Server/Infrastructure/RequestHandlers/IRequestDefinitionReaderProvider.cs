using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public interface IRequestDefinitionReaderProvider
    {
        IRequestDefinitionReader GetReader();
    }
}
