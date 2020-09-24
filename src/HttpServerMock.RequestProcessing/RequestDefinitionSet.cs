using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionSet
    {
        public RequestDefinitionSet(string definitionName, IEnumerable<RequestDefinition> definitions)
        {
            DefinitionName = definitionName;
            Definitions = definitions;
        }
        public string DefinitionName { get; }
        public IEnumerable<RequestDefinition> Definitions { get; }

    }
}
