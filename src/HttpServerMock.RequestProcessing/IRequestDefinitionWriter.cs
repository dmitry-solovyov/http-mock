using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionWriter
    {
        void Write(RequestDefinitionSet requestDefinitionSet, TextWriter textWriter);
    }
}
