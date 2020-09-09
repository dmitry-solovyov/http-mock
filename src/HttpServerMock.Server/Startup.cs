using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestProcessing.Yaml;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HttpServerMock.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRequestHistoryStorage, RequestHistoryStorage>();
            services.AddSingleton<IRequestDefinitionProvider, RequestDefinitionProvider>();
            services.AddTransient<IRequestDefinitionReader, YamlRequestDefinitionReader>();
            services.AddTransient<IRequestDefinitionWriter, YamlRequestDefinitionWriter>();
            services.AddTransient<IRequestDetailsProvider, RequestDetailsProvider>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName != Environments.Development)
            {
                app.UseUnhandledExceptionHandler();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestLogger();
            app.UseConfigurationHandler();
            app.UseMockedRequestHandler();
        }
    }
}
