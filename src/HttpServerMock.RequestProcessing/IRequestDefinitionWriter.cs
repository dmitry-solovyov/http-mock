using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionWriter
    {
        void Write(RequestDefinitionItemSet requestDefinitionSet, TextWriter textWriter);
    }
}
