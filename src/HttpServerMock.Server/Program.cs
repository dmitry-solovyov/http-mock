using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace HttpServerMock.Server
{
    public class Program
    {
        private const int DefaultHttpPort = 8888;
        private const int DefaultHttpsPort = 8899;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static void StartServer(string listenUri)
        {
            Console.WriteLine("Application starts");

            Host
                .CreateDefaultBuilder()
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
            Console.WriteLine("Application starts");

            return Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(BuildUrls(args));
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static string[] BuildUrls(string[] args)
        {
            if (args?.Any() != true)
                return Array.Empty<string>();

            var configurationBuilder = new ConfigurationBuilder()
                .Add(new CommandLineConfigurationSource { Args = args });

            var (port, server, schema) = GetStartupParameters(configurationBuilder.Build());
            var url = $"{schema}://{server}:{port}";

            return new[] { url };
        }

        private static (int port, string server, string schema) GetStartupParameters(IConfiguration configuration)
        {
            if (!int.TryParse(configuration["port"], out var port) || port <= 1000 || port > 65000)
            {
                port = DefaultHttpPort;
            }

            string server;
            if (string.IsNullOrWhiteSpace(server = configuration["server"]))
            {
                server = "*";
            }

            string schema;
            if (string.IsNullOrWhiteSpace(schema = configuration["schema"]))
            {
                schema = "http";
            }

            return (port, server, schema);
        }
    }
}
