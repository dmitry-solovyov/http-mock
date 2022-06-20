using HttpServerMock.RequestDefinitions;
using System.Net.Mime;

namespace HttpServerMock.RequestDefinitionProcessing.Json
{
    public class JsonRequestDefinitionReader : IRequestDefinitionReader
    {
        public string ContentType => MediaTypeNames.Application.Json;

        public ConfigurationDefinition Read(string requestContent)
        {
            return new ConfigurationDefinition();
        }
    }
}