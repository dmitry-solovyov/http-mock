namespace HttpMock.Logging;

public class CustomConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new CustomConsoleLogger(GetLastName(categoryName));

    private static string GetLastName(string input)
    {
        try
        {
            ReadOnlySpan<char> span = input.AsSpan();

            var lastDotIndex = span.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                return input;
            }

            ReadOnlySpan<char> lastNameSpan = span.Slice(lastDotIndex + 1);
            return lastNameSpan.ToString();
        }
        catch
        {
            return input;
        }
    }

    public void Dispose() { }
}