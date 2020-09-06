using System.Collections.Generic;
using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        IEnumerable<RequestDefinition> Read(TextReader textReader);
    }
}
