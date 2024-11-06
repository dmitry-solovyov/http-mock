using HttpMock.Models;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace HttpMock.Configuration;

public sealed class ConfigurationStorage : IConfigurationStorage
{
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private readonly ConcurrentDictionary<string, DomainConfiguration> _domainConfigurations = new();

    public int DomainsCount => _domainConfigurations.Count;

    public void ConfigureDomain(DomainConfiguration domainConfiguration)
    {
        ArgumentNullException.ThrowIfNull(domainConfiguration);
        ArgumentException.ThrowIfNullOrEmpty(domainConfiguration.Domain);

        _domainConfigurations.AddOrUpdate(domainConfiguration.Domain, domainConfiguration, (_, __) => domainConfiguration);
    }

    public bool IsDomainExists(string? domain)
    {
        if (string.IsNullOrEmpty(domain))
            return false;

        return _domainConfigurations.ContainsKey(domain);
    }

    public bool TryRemoveDomain(string? domain)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);

        return _domainConfigurations.TryRemove(domain, out _);
    }

    public bool TryGetDomainConfiguration(string? domain, out DomainConfiguration domainConfiguration)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);

        if (_domainConfigurations.TryGetValue(domain, out var foundDomain) && foundDomain != default)
        {
            domainConfiguration = foundDomain;
            return true;
        }

        domainConfiguration = default!;
        return false;
    }

    public IReadOnlyCollection<string> GetDomains() => _domainConfigurations.Keys.ToImmutableArray();

    public void ResetUsageCounters(string? domain)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);

        if (!_domainConfigurations.TryGetValue(domain, out var domainConfiguration))
            return;

        _semaphore.Wait();

        try
        {
            foreach (var endpointConfiguration in domainConfiguration.Endpoints)
            {
                endpointConfiguration.ResetCounter();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

}