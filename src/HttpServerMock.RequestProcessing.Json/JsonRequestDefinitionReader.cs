using HttpServerMock.RequestDefinitions;
using System.IO;
using System.Net.Mime;
using System.Threading;

namespace HttpServerMock.RequestDefinitionProcessing.Json
{
    public class JsonRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public RequestDefinitionItemSet Read(TextReader textReader, CancellationToken cancellationToken)
        {
            return new RequestDefinitionItemSet(string.Empty, new RequestDefinitionItem[0]);
        }
    }
}