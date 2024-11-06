using HttpMock.RequestProcessing;

namespace HttpMock;

public static class Server
{
    public static void Start(string[] args)
    {
        var startupParameters = ProgramArgumentsReader.GetStartupArguments(args);
        Start(startupParameters, args);
    }

    public static void Start(StartupArguments startupParameters, string[] args)
    {
        if (IsHelpRequested(args))
        {
            ShowHelp();
            return;
        }

        var (loggerProvider, logger) = ProgramSetupHelper.CreateLoggers();
        logger.LogInformation("Application starts");

        if (!IsPortValid(startupParameters.Port))
        {
            logger?.LogWarning("Binding port is not specified!");
            return;
        }

        logger?.LogInformation($"Binding port: {startupParameters.Port}");

        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = "HttpMock",
            Args = args,
            EnvironmentName = string.Empty
        });
        builder.WebHost.ConfigureKestrel(app => app.ListenAnyIP(startupParameters.Port));
        builder.WebHost.UseKestrelCore();

        builder.Logging.ClearProviders();
        if (!startupParameters.IsQuiet)
            builder.Logging.AddProvider(loggerProvider);

        ProgramSetupHelper.SetupApplicationServices(builder.Services);

        var app = builder.Build();

        app.UseUnhandledExceptionHandler();
        app.UseRequestPipeline();
        app.Run();
    }

    private static bool IsHelpRequested(string[] args)
    {
        return args.Length == 0 ||
               args.Contains("/?") ||
               args.Contains("?") ||
               args.Contains("--help") ||
               args.Contains("/help");
    }

    private static void ShowHelp()
    {
        Console.WriteLine("\nUsage:");
        Console.Write(
        """
        {nameof(HttpMock)} port=00000 [quiet=0]

        Parameters:
            port: (required) bind service to this port
            quiet: (optional) run the application without console output
        """);
        Console.ReadKey();
    }

    private static bool IsPortValid(int port) => port > 1023 && port <= 65535;
}
