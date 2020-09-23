using HttpServerMock.Server.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace HttpServerMock.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static void StartServer(string listenUri)
        {
            var (loggerProvider, logger) = CreateLoggers();

            logger.LogInformation("Application starts");

            Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(loggerProvider);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(listenUri);
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var (loggerProvider, logger) = CreateLoggers();

            logger.LogInformation("Application starts");

            return Host
                .CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(loggerProvider);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(BuildUrls(args, logger));
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static (ILoggerProvider loggerProvider, ILogger logger) CreateLoggers()
        {
            ILoggerProvider loggerProvider = new CustomConsoleLoggerProvider();

            ILoggerFactory loggerFactory = new LoggerFactory(new[] { loggerProvider });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            return (loggerProvider, logger);
        }

        private static string[] BuildUrls(string[] args, ILogger logger)
        {
            if (args?.Any() != true)
                return Array.Empty<string>();

            var configurationBuilder = new ConfigurationBuilder()
                .Add(new CommandLineConfigurationSource { Args = args });

            var urls = GetUrlParameters(configurationBuilder.Build());
            logger.LogInformation($"Binding urls: {string.Join(',', urls)}");

            return urls;
        }

        private static string[] GetUrlParameters(IConfiguration configuration)
        {
            var urls = configuration["urls"];
            if (string.IsNullOrWhiteSpace(urls))
            {
                return Array.Empty<string>();
            }

            var urlParts = urls.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return urlParts;
        }
    }
}
