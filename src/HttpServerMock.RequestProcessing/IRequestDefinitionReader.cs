using System.IO;
using System.Threading;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        RequestDefinitionItemSet Read(TextReader textReader, CancellationToken cancellationToken);
    }
}
