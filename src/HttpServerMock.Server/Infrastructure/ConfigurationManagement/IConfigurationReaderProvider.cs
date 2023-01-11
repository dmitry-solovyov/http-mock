using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public interface IConfigurationReaderProvider
{
    IConfigurationReader GetReader();
}
