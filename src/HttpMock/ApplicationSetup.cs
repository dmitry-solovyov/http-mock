﻿using HttpMock.Configuration;
using HttpMock.Logging;
using HttpMock.RequestHandlers.CommandRequestHandlers;
using HttpMock.RequestHandlers.MockedRequestHandlers;
using HttpMock.RequestProcessing;
using HttpMock.Serializations;

namespace HttpMock;

internal static class ApplicationSetup
{
    internal static (ILoggerProvider loggerProvider, ILogger logger) CreateLoggers()
    {
        ILoggerProvider loggerProvider = new CustomConsoleLoggerProvider();

        ILoggerFactory loggerFactory = new LoggerFactory([loggerProvider]);
        ILogger logger = loggerFactory.CreateLogger<Program>();

        return (loggerProvider, logger);
    }

    internal static void SetupApplicationServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddSingleton<IUnknownCommandHandler, UnknownCommandHandler>();
        services.AddSingleton<ConfigurationCommandHandler>();
        services.AddSingleton<UsageCountersCommandHandler>();
        services.AddSingleton<IMockedRequestHandler, MockedRequestHandler>();

        services.AddSingleton<ISerializationProvider, SerializationProvider>();
        services.AddSingleton<ISerialization, YamlSerialization>();

        services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();
        services.AddSingleton<IRequestRouter, RequestRouter>();
        services.AddSingleton<IMockedRequestEndpointConfigurationResolver, MockedRequestEndpointConfigurationResolver>();
    }
}
