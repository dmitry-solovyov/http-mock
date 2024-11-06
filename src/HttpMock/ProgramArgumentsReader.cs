using Microsoft.Extensions.Configuration.CommandLine;

namespace HttpMock;

public readonly record struct StartupArguments(int Port, bool IsQuiet);

public class ProgramArgumentsReader
{
    internal static StartupArguments GetStartupArguments(string[] args)
    {
        if (args?.Any() != true)
            return default;

        var configurationBuilder = new ConfigurationBuilder()
            .Add(new CommandLineConfigurationSource { Args = args });

        var configuration = configurationBuilder.Build();

        var isQuietValue = GetParameterInArgs(configuration, "quiet");
        var isQuiet = "true".Equals(isQuietValue, StringComparison.OrdinalIgnoreCase) ||
                      "1".Equals(isQuietValue, StringComparison.OrdinalIgnoreCase);

        var portValue = GetParameterInArgs(configuration, "port");
        int.TryParse(portValue, out var port);

        return new StartupArguments(port, isQuiet);
    }

    private static string? GetParameterInArgs(IConfiguration configuration, string parameterName)
    {
        var parameterValue = configuration[parameterName];
        if (string.IsNullOrWhiteSpace(parameterValue))
            return default;

        return parameterValue;
    }
}
