using System.IO;
using System.Threading;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        string ContentType { get; }
        RequestDefinitionItemSet Read(TextReader textReader, CancellationToken cancellationToken);
    }
}
