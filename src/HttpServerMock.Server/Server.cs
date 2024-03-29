using HttpServerMock.Server.Infrastructure.ConfigurationManagement;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationReaders;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.ConfigurationWriters;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using HttpServerMock.Server.Infrastructure.Logging;
using HttpServerMock.Server.Infrastructure.RequestProcessing;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;
using Microsoft.Extensions.Configuration.CommandLine;

namespace HttpServerMock.Server;

public static class Server
{
    public static void StartWebApp(string[] args)
    {
        Start(BuildUrls(args));
    }

    public static void StartTool(string bindingUrl)
    {
        Start(new[] { bindingUrl });
    }

    private static void Start(string[] bindingUrls)
    {
        var (loggerProvider, logger) = CreateLoggers();
        logger.LogInformation("Application starts");

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(loggerProvider);

        SetupServices(builder.Services);

        if (bindingUrls?.Length > 0)
        {
            bindingUrls = bindingUrls.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            logger.LogInformation($"Binding URLs: {string.Join(',', bindingUrls)}");

            builder.WebHost.UseUrls(bindingUrls);
            builder.WebHost.ConfigureKestrel(app =>
            {
                app.AllowSynchronousIO = true;
            });
        }
        else
        {
            logger.LogWarning("No binding URLs were specified!");
        }

        var app = builder.Build();

        _ = app.Environment.IsDevelopment()
            ? app.UseUnhandledExceptionHandler()
            : app.UseDeveloperExceptionPage();

        app.UseRequestLogger();
        app.UseRequestPipeline();

        app.Run();
    }

    public static void SetupServices(IServiceCollection services)
    {
        services.AddTransient<ConfigureCommandGetHandler>();
        services.AddTransient<ConfigureCommandPutHandler>();
        services.AddTransient<ResetConfigurationCommandHandler>();
        services.AddTransient<ResetCounterCommandHandler>();
        services.AddTransient<MockedRequestHandler>();

        services.AddTransient<IConfigurationReaderProvider, ConfigurationReaderProvider>();
        services.AddTransient<IConfigurationWriterProvider, ConfigurationWriterProvider>();

        services.AddTransient<IConfigurationReader, YamlConfigurationReader>();
        services.AddTransient<IConfigurationWriter, YamlConfigurationWriter>();
        services.AddTransient<IConfigurationReader, JsonConfigurationReader>();
        services.AddTransient<IConfigurationWriter, JsonConfigurationWriter>();
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<IHttpRequestDetailsProvider, HttpRequestDetailsProvider>();
        services.AddTransient<IRequestRouter, RequestRouter>();

        services.AddSingleton<IRequestHistoryStorage, RequestHistoryStorage>();
        services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();
    }

    public static (ILoggerProvider loggerProvider, ILogger logger) CreateLoggers()
    {
        ILoggerProvider loggerProvider = new CustomConsoleLoggerProvider();

        ILoggerFactory loggerFactory = new LoggerFactory(new[] { loggerProvider });
        ILogger logger = loggerFactory.CreateLogger<Program>();

        return (loggerProvider, logger);
    }

    private static string[] BuildUrls(string[] args)
    {
        if (args?.Any() != true)
            return Array.Empty<string>();

        var configurationBuilder = new ConfigurationBuilder()
            .Add(new CommandLineConfigurationSource { Args = args });

        var urls = GetUrlParameters(configurationBuilder.Build());
        return urls;
    }

    private static string[] GetUrlParameters(IConfiguration configuration)
    {
        const string urlsArgumentName = "urls";
        const char urlSeparator = ',';

        var urls = configuration[urlsArgumentName];
        if (string.IsNullOrWhiteSpace(urls))
            return Array.Empty<string>();

        var urlParts = urls
            .Split(urlSeparator)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        return urlParts;
    }
}
