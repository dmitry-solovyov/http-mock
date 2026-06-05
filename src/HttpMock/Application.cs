using HttpMock.RequestProcessing;

namespace HttpMock;

public static class Application
{
    private const int MIN_PORT = 1024;
    private const int MAX_PORT = 65535;

    public static void Start(string[] args)
    {
        var startupParameters = ProgramArgumentsReader.GetStartupArguments(args);
        if (startupParameters.IsHelpRequested)
        {
            ShowHelp();
            return;
        }

        var app = CreateWebApplication(startupParameters);
        app.Run();
    }

    public static WebApplication CreateWebApplication(StartupArguments startupParameters)
    {

        var (loggerProvider, logger) = ApplicationSetup.CreateLoggers();
        logger.LogInformation("Application starts");

        if (!IsPortValid(startupParameters.Port))
        {
            throw new InvalidOperationException("Port is not specified!");
        }

        logger?.LogInformation("Port: {Port}", startupParameters.Port);

        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            ApplicationName = "HttpMock",
            Args = [],
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty
        });
        builder.WebHost.ConfigureKestrel(app => app.ListenAnyIP(startupParameters.Port));
        builder.WebHost.UseKestrelCore();

        builder.Logging.ClearProviders();
        if (!startupParameters.IsQuiet)
            builder.Logging.AddProvider(loggerProvider);

        ApplicationSetup.SetupApplicationServices(builder.Services);

        var app = builder.Build();
        app.UseUnhandledExceptionHandler();
        app.UseMiddleware<RequestPipelineMiddleware>();
        return app;
    }

    private static void ShowHelp()
    {
        Console.WriteLine(
        $"""
        Usage:
        {nameof(HttpMock).ToLower()} --port=00000 [--quiet=0]

        Parameters:
            port: (required) bind service to this port (allowed range: {MIN_PORT} .. {MAX_PORT})
            quiet: (optional) run the application without console output

        Press any key . . .
        """);
        Console.ReadKey();
    }

    private static bool IsPortValid(int port) => port >= MIN_PORT && port <= MAX_PORT;
}
