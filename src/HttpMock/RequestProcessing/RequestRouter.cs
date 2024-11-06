using HttpMock.RequestHandlers.CommandRequestHandlers;
using HttpMock.RequestHandlers.MockedRequestHandlers;

namespace HttpMock.RequestProcessing;

public readonly record struct RouteDetails(RequestDetails RequestDetails, IRequestHandler RequestHandler);

public interface IRequestRouter
{
    bool TryGetRouteDetails(HttpContext httpContext, out RouteDetails routeDetails);
}

public partial class RequestRouter : IRequestRouter
{
    private readonly ILogger<RequestRouter> _logger;
    private readonly IRequestDetailsProvider _htpRequestDetailsProvider;
    private readonly ICommandRequestHandlerProvider _commandRequestHandlerProvider;

    public RequestRouter(
        ILogger<RequestRouter> logger,
        IRequestDetailsProvider httpRequestDetailsProvider,
        ICommandRequestHandlerProvider commandRequestHandlerProvider)
    {
        _logger = logger;
        _htpRequestDetailsProvider = httpRequestDetailsProvider;
        _commandRequestHandlerProvider = commandRequestHandlerProvider;
    }

    public bool TryGetRouteDetails(HttpContext httpContext, out RouteDetails routeDetails)
    {
        if (!_htpRequestDetailsProvider.TryGetRouteDetails(httpContext, out var requestDetails))
        {
            routeDetails = default;
            return false;
        }

        if (TryGetCommandHandler(httpContext, ref requestDetails, out var commandHandler))
        {
            routeDetails = new RouteDetails(requestDetails, commandHandler!);
            _logger.LogDebug($"Command: {requestDetails.CommandName} (handler: {commandHandler!.GetType().Name})");
            return true;
        }

        if (string.IsNullOrEmpty(requestDetails.Domain))
        {
            routeDetails = default;
            return false;
        }

        var requestHandler = httpContext.RequestServices.GetRequiredService<MockedRequestHandler>();
        routeDetails = new RouteDetails(requestDetails, requestHandler);
        return true;
    }

    private bool TryGetCommandHandler(HttpContext httpContext, ref readonly RequestDetails requestDetails, out ICommandRequestHandler? commandHandler)
    {
        commandHandler = default;

        if (string.IsNullOrEmpty(requestDetails.CommandName))
        {
            return false;
        }

        var commandHandlerType = _commandRequestHandlerProvider.GetCommandHandlerType(requestDetails.CommandName, requestDetails.HttpMethod);
        if (commandHandlerType != default)
        {
            commandHandler = httpContext.RequestServices.GetRequiredService(commandHandlerType) as ICommandRequestHandler;
        }

        return commandHandler != default;
    }
}