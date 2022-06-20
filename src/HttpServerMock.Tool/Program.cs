using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using System;
using System.Linq;
using System.Reflection;

namespace HttpServerMock.Tool
{
    internal class Program
    {
        private const int DefaultHttpPort = 8888;

        public static void Main(string[] args)
        {
            if (args.Length == 0 ||
                args.Contains("/?") ||
                args.Contains("?") ||
                args.Contains("--help") ||
                args.Contains("/help"))
            {
                ShowHelp();
                return;
            }

            var versionString = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException())
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            Console.WriteLine($"Starting {nameof(HttpServerMock)} v{versionString}");

            var configurationBuilder = new ConfigurationBuilder()
                .Add(new CommandLineConfigurationSource { Args = args });

            var (port, server, schema) = GetStartupParameters(configurationBuilder.Build());
            var url = $"{schema}://{server}:{port}";

            Console.WriteLine($"Starting server for url: {url}...");

            Server.Server.StartTool(url);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("\nUsage:");
            Console.Write($"  {nameof(HttpServerMock)}.{nameof(Tool)}");
            Console.WriteLine(@" --server * --port 8888 --schema http

Parameters:
    --port: (required) start service by this port
        Value:
            0000: port number in range 00...000
            8888 - default value

    --server: (optional) start service with endpoint name bind
        Value:
            * (default)- bind any endpoint to the service
            localhost (or 127.0.0.1) - bind name of the local host
            server.url - name that can be bind to the service

    --schema: (optional) start service using
        Value:
            http (default)
            https
");
            Console.ReadKey();
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