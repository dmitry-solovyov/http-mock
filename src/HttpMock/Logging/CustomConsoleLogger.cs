namespace HttpMock.Logging;

public class CustomConsoleLogger : ILogger
{
    public CustomConsoleLogger() { }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"{DateTime.UtcNow:O} [{GetColoredText(GetLogLevelShortName(logLevel), GetLogLevelColor(logLevel))}] {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    private static string GetLogLevelShortName(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            LogLevel.None => "NON",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };

    private static string GetLogLevelColor(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => ConsoleAsciiColors.NORMAL,
            LogLevel.Debug => ConsoleAsciiColors.GREY,
            LogLevel.Information => ConsoleAsciiColors.GREEN,
            LogLevel.Warning => ConsoleAsciiColors.MAGENTA,
            LogLevel.Error => ConsoleAsciiColors.RED,
            LogLevel.Critical => ConsoleAsciiColors.RED,
            _ => ConsoleAsciiColors.NORMAL
        };

    public static class ConsoleAsciiColors
    {
        public static string NORMAL => Console.IsOutputRedirected ? string.Empty : "\x1b[39m";
        public static string RED => Console.IsOutputRedirected ? string.Empty : "\x1b[91m";
        public static string GREEN => Console.IsOutputRedirected ? string.Empty : "\x1b[92m";
        public static string YELLOW => Console.IsOutputRedirected ? string.Empty : "\x1b[93m";
        public static string BLUE => Console.IsOutputRedirected ? string.Empty : "\x1b[94m";
        public static string MAGENTA => Console.IsOutputRedirected ? string.Empty : "\x1b[95m";
        public static string CYAN => Console.IsOutputRedirected ? string.Empty : "\x1b[96m";
        public static string GREY => Console.IsOutputRedirected ? string.Empty : "\x1b[97m";
    }

    public static class ConsoleAsciiStyles
    {
        public static string BOLD => Console.IsOutputRedirected ? string.Empty : "\x1b[1m";
        public static string NOBOLD => Console.IsOutputRedirected ? string.Empty : "\x1b[22m";
        public static string UNDERLINE => Console.IsOutputRedirected ? string.Empty : "\x1b[4m";
        public static string NOUNDERLINE => Console.IsOutputRedirected ? string.Empty : "\x1b[24m";
        public static string REVERSE => Console.IsOutputRedirected ? string.Empty : "\x1b[7m";
        public static string NOREVERSE => Console.IsOutputRedirected ? string.Empty : "\x1b[27m";
    }

    public static string GetColoredText(string text, string style)
    {
        if (Console.IsOutputRedirected)
            return text;

        return $"{style}{text}{ConsoleAsciiColors.NORMAL}";
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
}