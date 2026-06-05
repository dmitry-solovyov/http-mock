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
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private Models.Configuration? _configuration = default;

    public void SetConfiguration(Models.Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _semaphore.Wait();
        try
        {
            _configuration = configuration;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void RemoveConfiguration()
    {
        if (_configuration == null)
            return;

        _semaphore.Wait();
        try
        {
            _configuration = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public bool TryGetConfiguration(out Models.Configuration configuration)
    {
        _semaphore.Wait();
        try
        {
            if (_configuration == null)
            {
                configuration = new Models.Configuration([]);
                return false;
            }

            configuration = _configuration;
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void ResetUsageCounters()
    {
        if (_configuration == null)
            return;

        _semaphore.Wait();
        try
        {
            if (_configuration != null)
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