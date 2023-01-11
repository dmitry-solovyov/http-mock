using HttpServerMock.Server.Infrastructure.RequestProcessing;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public class ConfigurationStorage : IConfigurationStorage
{
    private readonly ReaderWriterLockSlim _listLock = new ReaderWriterLockSlim();

    private readonly IList<ConfigurationStorageItemSet> _requestDefinitionSets = new List<ConfigurationStorageItemSet>();

    public int GetCount() => _requestDefinitionSets.Count;

    public void Clear()
    {
        _listLock.EnterWriteLock();
        try
        {
            _requestDefinitionSets.Clear();
        }
        finally
        {
            _listLock.ExitWriteLock();
        }
    }

    public void AddSet(ConfigurationStorageItemSet definitionSet)
    {
        _listLock.EnterWriteLock();
        try
        {
            _requestDefinitionSets.Clear();
            _requestDefinitionSets.Add(definitionSet);
        }
        finally
        {
            _listLock.ExitWriteLock();
        }
    }

    public ConfigurationStorageItem? FindItem(ref RequestContext requestContext)
    {
        _listLock.EnterReadLock();

        try
        {
            Collection<ConfigurationStorageItem>? foundItems = null;
            var counter = 0;

            foreach (var requestDefinitionSet in _requestDefinitionSets)
            {
                foreach (var requestDefinition in requestDefinitionSet.DefinitionItems)
                {
                    if (string.IsNullOrWhiteSpace(requestDefinition.When.Url) ||
                        string.IsNullOrWhiteSpace(requestDefinition.When.UrlRegexExpression))
                        continue;

                    if (!string.Equals(
                        requestContext.HttpRequestDetails.Url,
                        requestDefinition.When.Url,
                        requestDefinition.When.CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    {
                        var regexOptions = RegexOptions.Singleline;
                        if (requestDefinition.When.CaseInsensitive)
                            regexOptions |= RegexOptions.IgnoreCase;

                        var match = Regex.Match(requestContext.HttpRequestDetails.Url, requestDefinition.When.UrlRegexExpression, regexOptions);
                        if (!match.Success)
                            continue;
                    }

                    foundItems ??= new Collection<ConfigurationStorageItem>();
                    foundItems.Add(requestDefinition);
                    counter++;

                    if (counter == requestContext.Counter)
                    {
                        return requestDefinition;
                    }
                }
            }

            if (foundItems?.Count > 0)
            {
                var index = CalculateIndexByCounter(requestContext.Counter, foundItems.Count);
                return foundItems[index];
            }

            return null;
        }
        finally
        {
            _listLock.ExitReadLock();
        }
    }

    public IReadOnlyCollection<ConfigurationStorageItemSet> GetDefinitionSets()
    {
        return _requestDefinitionSets.ToImmutableArray();
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