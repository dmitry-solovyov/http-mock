namespace HttpServerMock.Server.Infrastructure.Logging
{
    public class CustomConsoleLogger : ILogger
    {
        private readonly string _categoryName;

        public CustomConsoleLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Console.WriteLine($"{DateTime.UtcNow:O} [{logLevel}][{_categoryName}]{Environment.NewLine}{new string(' ', 4)}{formatter(state, exception)}{Environment.NewLine}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}