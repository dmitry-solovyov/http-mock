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

            var urls = GetUrlParameters(configurationBuilder.Build());
            Console.WriteLine($"Binding urls: {string.Join(',', urls)}");

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
