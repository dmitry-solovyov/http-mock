using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;
using System.Collections.Generic;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDefinitionProvider
    {
        int Count { get; }
    
        void Clear();
        void AddSet(RequestDefinitionItemSet definitionSet);
        RequestDefinitionItem[] FindItems(RequestContext request);
        IEnumerable<RequestDefinitionItemSet> GetDefinitionSets();
    }
}
