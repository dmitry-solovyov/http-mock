using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDefinitionProvider : IRequestDefinitionProvider
    {
        private readonly List<RequestDefinitionSet> _requestDefinitionSets = new List<RequestDefinitionSet>();

        public void AddRange(RequestDefinitionSet definitionSet)
        {
            _requestDefinitionSets.Clear();
            _requestDefinitionSets.Add(definitionSet);
        }

        public RequestDefinition[] FindItems(MockedRequest request)
        {
            var result = new List<RequestDefinition>();

            var context = request.RequestDetails;

            foreach (var requestDefinitionSet in _requestDefinitionSets)
            {
                foreach (var requestDefinition in requestDefinitionSet.Definitions)
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

        public IEnumerable<RequestDefinitionSet> GetItems()
        {
            return _requestDefinitionSets.AsEnumerable();
        }
    }
}