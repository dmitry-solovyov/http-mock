using HttpServerMock.RequestDefinitionProcessing.Json;
using HttpServerMock.RequestDefinitionProcessing.Yaml;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Infrastructure.RequestHandlers;
using HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ConfigureCommandGetHandler>();
            services.AddTransient<ConfigureCommandPutHandler>();
            services.AddTransient<ResetConfigurationCommandHandler>();
            services.AddTransient<ResetCounterCommandHandler>();
            services.AddTransient<MockedRequestHandler>();

            services.AddTransient<IRequestDefinitionReaderProvider, RequestDefinitionReaderProvider>();
            services.AddTransient<IRequestDefinitionWriterProvider, RequestDefinitionWriterProvider>();

            services.AddTransient<IRequestDefinitionReader, YamlRequestDefinitionReader>();
            services.AddTransient<IRequestDefinitionWriter, YamlRequestDefinitionWriter>();
            services.AddTransient<IRequestDefinitionReader, JsonRequestDefinitionReader>();
            services.AddTransient<IRequestDefinitionWriter, JsonRequestDefinitionWriter>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IRequestDetailsProvider, RequestDetailsProvider>();
            services.AddTransient<IRequestHandlerFactory, RequestHandlerFactory>();

            services.AddSingleton<IRequestHistoryStorage, RequestHistoryStorage>();
            services.AddSingleton<IRequestDefinitionStorage, RequestDefinitionStorage>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName != Environments.Development)
                app.UseUnhandledExceptionHandler();
            else
                app.UseDeveloperExceptionPage();

            app.UseRequestLogger();
            app.UseRequestPipeline();
        }
    }
}