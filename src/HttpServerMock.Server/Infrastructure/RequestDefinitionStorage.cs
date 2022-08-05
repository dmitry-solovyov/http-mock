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
            try
            {
                _requestDefinitionSets.Clear();
            }
            finally
            {
                _listLock.ExitReadLock();
            }
        }

        public void AddSet(RequestDefinitionItemSet definitionSet)
        {
            _listLock.EnterReadLock();
            try
            {
                _requestDefinitionSets.Clear();
                _requestDefinitionSets.Add(definitionSet);
            }
            finally
            {
                _listLock.ExitReadLock();
            }
        }

        public RequestDefinitionItem? FindItem(ref RequestContext requestContext)
        {
            var foundItems = new Collection<RequestDefinitionItem>();
            var counter = 0;

            foreach (var requestDefinitionSet in _requestDefinitionSets)
            {
                foreach (var requestDefinition in requestDefinitionSet.DefinitionItems)
                {
                    if (string.IsNullOrWhiteSpace(requestDefinition.When.Url) ||
                        string.IsNullOrWhiteSpace(requestDefinition.When.UrlRegexExpression))
                        continue;

                    if (!string.Equals(
                        requestContext.RequestDetails.Url,
                        requestDefinition.When.Url,
                        requestDefinition.When.CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    {
                        var regexOptions = RegexOptions.Singleline;
                        if (requestDefinition.When.CaseInsensitive)
                            regexOptions |= RegexOptions.IgnoreCase;

                        var match = Regex.Match(requestContext.RequestDetails.Url, requestDefinition.When.UrlRegexExpression, regexOptions);
                        if (!match.Success)
                            continue;
                    }

                    foundItems.Add(requestDefinition);
                    counter++;

                    if (counter == requestContext.Counter)
                    {
                        return requestDefinition;
                    }
                }
            }

            if (foundItems.Count == 0)
                return null;

            var index = CalculateIndexByCounter(requestContext.Counter, foundItems.Count);
            return foundItems[index];
        }

        public IReadOnlyCollection<RequestDefinitionItemSet> GetDefinitionSets()
        {
            return new ReadOnlyCollection<RequestDefinitionItemSet>(_requestDefinitionSets);
        }

        private int CalculateIndexByCounter(int counter, int totalItemsCount)
        {
            var index = counter <= 0 ? 0 : counter - 1;
            if (index >= totalItemsCount)
            {
                index %= totalItemsCount;
            }

            return index;
        }
    }
}