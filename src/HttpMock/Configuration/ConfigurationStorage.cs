namespace HttpMock.Configuration;

public interface IConfigurationStorage
{
    void SetConfiguration(Models.Configuration configuration);
    bool TryGetConfiguration(out Models.Configuration configuration);
    void RemoveConfiguration();
    public void ResetUsageCounters();
}


public sealed class ConfigurationStorage : IConfigurationStorage
{
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private Models.Configuration? _configuration = default;

    public void SetConfiguration(Models.Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
    }

    public void RemoveConfiguration()
    {
        _configuration = null;
    }

    public bool TryGetConfiguration(out Models.Configuration configuration)
    {
        if (_configuration == null)
        {
            configuration = new Models.Configuration([]);
            return false;
        }

        configuration = _configuration;
        return true;
    }

    public void ResetUsageCounters()
    {
        if (_configuration == null)
            return;

        _semaphore.Wait();
        try
        {
            foreach (var endpointConfiguration in _configuration.Endpoints)
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