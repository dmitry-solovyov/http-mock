using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionItemSet
    {
        public RequestDefinitionItemSet(string? definitionName, IEnumerable<RequestDefinitionItem> definitionItems)
        {
            DefinitionName = definitionName;
            DefinitionItems = definitionItems.ToArray();
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? DefinitionName { get; }
        public IReadOnlyCollection<RequestDefinitionItem> DefinitionItems { get; }
    }
}