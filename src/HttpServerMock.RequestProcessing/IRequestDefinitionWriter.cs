using System.Collections.Generic;
using System.IO;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionWriter
    {
        void Read(IEnumerable<RequestDefinition> requestDefinitions, TextWriter textWriter);
    }
}
