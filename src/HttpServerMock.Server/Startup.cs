using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestProcessing.Yaml;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

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
            services.AddTransient<IRequestDefinitionReader, YamlRequestDefinitionReader>();
            services.AddTransient<IRequestDefinitionWriter, YamlRequestDefinitionWriter>();

            AddRequestHandlers(services);

            services.AddScoped<IRequestDetailsProvider, RequestDetailsProvider>();

            services.AddSingleton<IRequestHistoryStorage, RequestHistoryStorage>();
            services.AddSingleton<IRequestDefinitionProvider, RequestDefinitionProvider>();
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

            ConfigureHandlerMiddleware(app);
        }

        private static void AddRequestHandlers(IServiceCollection services)
        {
            var types = typeof(Startup).Assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.GetInterfaces().Contains(typeof(IRequestHandler)))
                    continue;

                services.AddTransient(type);
                services.AddTransient(typeof(RequestPipelineMiddleware<>).MakeGenericType(type));
            }
        }

        private static void ConfigureHandlerMiddleware(IApplicationBuilder app)
        {
            var types = typeof(Startup).Assembly.GetTypes();

            foreach (var type in types)
            {
                if (!type.GetInterfaces().Contains(typeof(IRequestHandler)))
                    continue;

                app.UseMiddleware(typeof(RequestPipelineMiddleware<>).MakeGenericType(type));
            }
        }
    }
}
