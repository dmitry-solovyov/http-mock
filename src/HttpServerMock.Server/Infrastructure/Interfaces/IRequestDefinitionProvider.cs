using System.Collections.Generic;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.Interfaces
{
    public interface IRequestDefinitionProvider
    {
        void AddRange(RequestDefinitionSet definitionSet);
        RequestDefinition[] FindItems(MockedRequest request);
        IEnumerable<RequestDefinitionSet> GetItems();
    }
}
