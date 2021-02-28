using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDefinitionStorage : IRequestDefinitionStorage
    {
        private readonly List<RequestDefinitionItemSet> _requestDefinitionSets = new List<RequestDefinitionItemSet>();

        public int Count => _requestDefinitionSets.Count;

        public void Clear()
        {
            _requestDefinitionSets.Clear();
        }

        public void AddSet(RequestDefinitionItemSet definitionSet)
        {
            var foundItems = _requestDefinitionSets
                .Where(x => string.Equals(x.DefinitionName, definitionSet.DefinitionName))
                .ToArray();

            foreach (var foundItem in foundItems)
            {
                _requestDefinitionSets.Remove(foundItem);
            }

            _requestDefinitionSets.Add(definitionSet);
        }

        public RequestDefinitionItem[] FindItems(RequestContext request)
        {
            var result = new List<RequestDefinitionItem>();

            var context = request.RequestDetails;

            foreach (var requestDefinitionSet in _requestDefinitionSets)
            {
                foreach (var requestDefinition in requestDefinitionSet.DefinitionItems)
                {
                    if (string.IsNullOrWhiteSpace(requestDefinition.When.Url) ||
                        string.IsNullOrWhiteSpace(requestDefinition.When.UrlRegexExpression))
                        continue;

                    var regexOptions = RegexOptions.Singleline;
                    if (requestDefinition.When.CaseInsensitive)
                    {
                        regexOptions |= RegexOptions.IgnoreCase;
                    }

                    var match = Regex.Match(context.Uri, requestDefinition.When.UrlRegexExpression, regexOptions);
                    if (!match.Success)
                        continue;

                    result.Add(requestDefinition);
                }
            }

            return result.ToArray();
        }

        public IEnumerable<RequestDefinitionItemSet> GetDefinitionSets()
        {
            return _requestDefinitionSets.AsEnumerable();
        }
    }
}