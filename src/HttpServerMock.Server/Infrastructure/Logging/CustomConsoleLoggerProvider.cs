using Microsoft.Extensions.Logging;

namespace HttpServerMock.Server.Infrastructure.Logging
{
    public class CustomConsoleLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }
    }
}
