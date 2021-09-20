using System.Collections.Concurrent;
using HttpServerMock.RequestDefinitions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestDefinitionStorage : IRequestDefinitionStorage
    {
        private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim();

        private readonly ConcurrentDictionary<string, List<RequestDefinitionItemSet>> _requestDefinitionSets =
                new ConcurrentDictionary<string, List<RequestDefinitionItemSet>>();

        public IEnumerable<string> Departments => _requestDefinitionSets.Keys;

        public int GetCount(string department) => _requestDefinitionSets.Count;

        public void Clear(string department)
        {
            _requestDefinitionSets.TryRemove(department, out _);
        }

        public void AddSet(string department, RequestDefinitionItemSet definitionSet)
        {
            Clear(department);

            _listLock.EnterReadLock();
            try
            {
                var foundItems = _requestDefinitionSets
                .Where(x => string.Equals(x.DefinitionName, definitionSet.DefinitionName))
                .ToArray();

                foreach (var foundItem in foundItems)
                    _requestDefinitionSets.Remove(foundItem);

                _requestDefinitionSets.Add(definitionSet);
            }
            finally
            {
                _listLock.ExitReadLock();
            }
        }

        public IEnumerable<RequestDefinitionItem> FindItems(string department, RequestContext request, CancellationToken cancellationToken)
        {
            var context = request.RequestDetails;

            foreach (var requestDefinitionSet in _requestDefinitionSets.Values)
            {
                foreach (var requestDefinition in requestDefinitionSet.AsReadOnly())
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

        public IEnumerable<RequestDefinitionItemSet> GetDefinitionSets(string department)
        {
            return _requestDefinitionSets.AsReadOnly();
        }
    }
}