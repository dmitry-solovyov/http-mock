using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        RequestDefinitionItemSet Read(TextReader textReader);
    }
}
