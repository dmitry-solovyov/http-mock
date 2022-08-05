using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionStorage
    {
        int GetCount();

        void Clear();

        void AddSet(RequestDefinitionItemSet definitionSet);

        RequestDefinitionItem? FindItem(ref RequestContext request);

        IReadOnlyCollection<RequestDefinitionItemSet> GetDefinitionSets();
    }
}