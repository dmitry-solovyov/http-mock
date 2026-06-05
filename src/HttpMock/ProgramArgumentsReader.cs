using Microsoft.Extensions.Configuration.CommandLine;

namespace HttpMock;

public readonly record struct StartupArguments(int Port, bool IsQuiet, bool IsHelpRequested);

public static class ProgramArgumentsReader
{
    internal static StartupArguments GetStartupArguments(string[] args)
    {
        if (args?.Any() != true)
            return new StartupArguments(0, false, true);

        var configurationBuilder = new ConfigurationBuilder()
            .Add(new CommandLineConfigurationSource { Args = args });

        var configuration = configurationBuilder.Build();

        var isQuietValue = GetParameterInArgs(configuration, "quiet");
        var isQuiet = "true".Equals(isQuietValue, StringComparison.OrdinalIgnoreCase) ||
                      "1".Equals(isQuietValue, StringComparison.OrdinalIgnoreCase);

        var portValue = GetParameterInArgs(configuration, "port");
        int.TryParse(portValue, out var port);

        var isHelpRequested = IsHelpRequested(args);

        return new StartupArguments(port, isQuiet, isHelpRequested);
    }

    private static string? GetParameterInArgs(IConfiguration configuration, string parameterName)
    {
        var parameterValue = configuration[parameterName];
        if (string.IsNullOrWhiteSpace(parameterValue))
            return default;

        return parameterValue;
    }

    private static bool IsHelpRequested(string[] args)
    {
        return args.Contains("/?") ||
               args.Contains("?") ||
               args.Contains("--help") ||
               args.Contains("/help");
    }

}
