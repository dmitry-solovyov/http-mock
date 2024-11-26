using HttpMock.RequestProcessing;

namespace HttpMock;

public static class Server
{
    public static void Start(string[] args)
    {
        if (IsHelpRequested(args))
        {
            ShowHelp();
            return;
        }

        var app = CreateWebApplication(args);
        app.Run();
    }

    public static WebApplication CreateWebApplication(string[] args)
    {
        var startupParameters = ProgramArgumentsReader.GetStartupArguments(args);

        var (loggerProvider, logger) = ProgramSetupHelper.CreateLoggers();
        logger.LogInformation("Application starts");

        if (!IsPortValid(startupParameters.Port))
        {
            throw new InvalidOperationException("Binding port is not specified!");
        }

        logger?.LogInformation($"Binding port: {startupParameters.Port}");

        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = "HttpMock",
            Args = args,
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty
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
        return app;
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
