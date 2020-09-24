using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        RequestDefinitionSet Read(TextReader textReader);
    }
}
