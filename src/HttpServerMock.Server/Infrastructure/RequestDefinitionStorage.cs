using HttpServerMock.RequestDefinitions;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDefinitionStorage : IRequestDefinitionStorage
    {
        private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim();

        private readonly IList<RequestDefinitionItemSet> _requestDefinitionSets = new List<RequestDefinitionItemSet>();

        public int GetCount() => _requestDefinitionSets.Count;

        public void Clear()
        {
            _requestDefinitionSets.Clear();
        }

        public void AddSet(RequestDefinitionItemSet definitionSet)
        {
            Clear();

            _listLock.EnterReadLock();
            try
            {
                var foundItems = _requestDefinitionSets.ToArray();

                foreach (var foundItem in foundItems)
                    _requestDefinitionSets.Remove(foundItem);

                _requestDefinitionSets.Add(definitionSet);
            }
            finally
            {
                _listLock.ExitReadLock();
            }
        }

        public IEnumerable<RequestDefinitionItem> FindItems(RequestContext request)
        {
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

                    yield return requestDefinition;
                }
            }
        }

        public IReadOnlyCollection<RequestDefinitionItemSet> GetDefinitionSets()
        {
            return new ReadOnlyCollection<RequestDefinitionItemSet>(_requestDefinitionSets);
        }
    }
}