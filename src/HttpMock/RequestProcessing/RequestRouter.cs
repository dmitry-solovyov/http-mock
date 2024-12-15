using HttpMock.Models;
using HttpMock.RequestHandlers.CommandRequestHandlers;

namespace HttpMock.RequestProcessing;

public interface IRequestRouter
{
    bool TryGetMockedRouteDetails(HttpContext httpContext, out MockedRouteDetails routeDetails);
    bool TryGetCommandRouteDetails(HttpContext httpContext, out CommandRouteDetails commandRouteDetails);
}

public partial class RequestRouter : IRequestRouter
{
    private readonly IRequestDetailsProvider _htpRequestDetailsProvider;

    public RequestRouter(IRequestDetailsProvider httpRequestDetailsProvider)
    {
        _htpRequestDetailsProvider = httpRequestDetailsProvider;
    }

    public bool TryGetMockedRouteDetails(HttpContext httpContext, out MockedRouteDetails routeDetails)
    {
        if (!_htpRequestDetailsProvider.TryGetRequestDetails(httpContext, out var mockedRequestDetails))
        {
            routeDetails = default;
            return false;
        }

        var requestHandler = httpContext.RequestServices.GetRequiredService<IMockedRequestHandler>();
        routeDetails = new MockedRouteDetails(mockedRequestDetails, requestHandler);
        return true;
    }

    public bool TryGetCommandRouteDetails(HttpContext httpContext, out CommandRouteDetails commandRouteDetails)
    {
        if (!_htpRequestDetailsProvider.TryGetCommandRequestDetails(httpContext, out var commandRequestDetails))
        {
            commandRouteDetails = default;
            return false;
        }

        var commandHandlerType = GetCommandRequestHandlerType(commandRequestDetails.CommandName);
        var commandHandler = (ICommandRequestHandler)httpContext.RequestServices.GetRequiredService(commandHandlerType);
        commandRouteDetails = new CommandRouteDetails(commandRequestDetails, commandHandler);
        return true;
    }

    private static Type GetCommandRequestHandlerType(ReadOnlySpan<char> commandName)
    {
        return commandName switch
        {
            var cmd when IsCommandName(cmd, DomainsCommandHandler.CommandName) => typeof(DomainsCommandHandler),
            var cmd when IsCommandName(cmd, DomainConfigurationCommandHandler.CommandName) => typeof(DomainConfigurationCommandHandler),
            var cmd when IsCommandName(cmd, UsageCountersCommandHandler.CommandName) => typeof(UsageCountersCommandHandler),

            _ => typeof(UnknownCommandHandler)
        };
    }

    private static bool IsCommandName(ReadOnlySpan<char> requestedCommand, ReadOnlySpan<char> expectedCommand) =>
        expectedCommand.SequenceEqual(requestedCommand, CharComparer.OrdinalIgnoreCase);
}