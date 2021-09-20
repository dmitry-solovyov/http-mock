using System.Collections.Generic;
using System.Threading;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionStorage
    {
        IEnumerable<string> Departments { get; }

        int GetCount(string department);
        void Clear(string department);
        void AddSet(string department, RequestDefinitionItemSet definitionSet);
        IEnumerable<RequestDefinitionItem> FindItems(string department, RequestContext request, CancellationToken cancellationToken);
        IEnumerable<RequestDefinitionItemSet> GetDefinitionSets(string department);
    }
}
