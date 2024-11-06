using HttpMock.Models;

namespace HttpMock.Configuration;

public interface IConfigurationStorage
{
    int DomainsCount { get; }

    bool IsDomainExists(string? domain);

    void ConfigureDomain(DomainConfiguration domainConfiguration);

    bool TryRemoveDomain(string? domain);

    bool TryGetDomainConfiguration(string? domain, out DomainConfiguration domainConfiguration);

    IReadOnlyCollection<string> GetDomains();

    public void ResetUsageCounters(string? domain);
}
