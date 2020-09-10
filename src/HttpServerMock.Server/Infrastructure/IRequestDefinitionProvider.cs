using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HttpServerMock.Server.Infrastructure
{
    public interface IRequestDefinitionProvider
    {
        void AddRange(IEnumerable<RequestDefinition> definitions);
        RequestDefinition[] FindItems(MockedRequest request);
        IEnumerable<RequestDefinition> GetItems();
    }

    public class RequestDefinitionProvider : IRequestDefinitionProvider
    {
        private readonly List<RequestDefinition> _requestDefinitions = new List<RequestDefinition>();

        public void AddRange(IEnumerable<RequestDefinition> definitions)
        {
            _requestDefinitions.AddRange(definitions);
        }

        public RequestDefinition[] FindItems(MockedRequest request)
        {
            var result = new List<RequestDefinition>();

            var context = request.RequestDetails;

            foreach (var requestDefinition in _requestDefinitions)
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

            return result.ToArray();
        }

        public IEnumerable<RequestDefinition> GetItems()
        {
            return _requestDefinitions.AsEnumerable();
        }
    }
}
