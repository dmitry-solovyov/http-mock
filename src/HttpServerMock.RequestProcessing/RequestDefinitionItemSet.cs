using System;
using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionItemSet
    {
        public RequestDefinitionItemSet(string? definitionName, IEnumerable<RequestDefinitionItem> definitionItems)
        {
            DefinitionName = definitionName;
            DefinitionItems = definitionItems;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? DefinitionName { get; }
        public IEnumerable<RequestDefinitionItem> DefinitionItems { get; }

    }
}
