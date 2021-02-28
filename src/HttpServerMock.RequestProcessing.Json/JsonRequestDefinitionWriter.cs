using HttpServerMock.RequestDefinitions;
using System.IO;

namespace HttpServerMock.RequestDefinitionProcessing.Json
{
    public class JsonRequestDefinitionWriter : IRequestDefinitionWriter
    {
        public string ContentType => "application/json";

        public void Write(RequestDefinitionItemSet requestDefinitionSet, TextWriter textWriter)
        {
        }
    }
}
