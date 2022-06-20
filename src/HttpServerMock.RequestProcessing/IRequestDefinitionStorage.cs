using System.Collections.Generic;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionStorage
    {
        int GetCount();

        void Clear();

        void AddSet(RequestDefinitionItemSet definitionSet);

        IEnumerable<RequestDefinitionItem> FindItems(RequestContext request);

        IReadOnlyCollection<RequestDefinitionItemSet> GetDefinitionSets();
    }
}