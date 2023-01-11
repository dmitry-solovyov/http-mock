using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement;

public interface IConfigurationWriterProvider
{
    IConfigurationWriter GetWriter();
}
