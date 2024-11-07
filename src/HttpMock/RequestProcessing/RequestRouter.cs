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
    private readonly IRequestDetailsProvider _htpRequestDetailsProvider;

    public RequestRouter(IRequestDetailsProvider httpRequestDetailsProvider)
    {
        _htpRequestDetailsProvider = httpRequestDetailsProvider;
    }

    public bool TryGetRouteDetails(HttpContext httpContext, out RouteDetails routeDetails)
    {
        if (!_htpRequestDetailsProvider.TryGetRequestDetails(httpContext, out var requestDetails))
        {
            routeDetails = default;
            return false;
        }

        var requestHandlerType = GetRequestHandlerType(ref requestDetails);

        var requestHandler = requestHandlerType != default
            ? httpContext.RequestServices.GetRequiredService(requestHandlerType) as IRequestHandler
            : default;

        routeDetails = requestHandler != default ? new RouteDetails(requestDetails, requestHandler) : default;
        return requestHandler != default;
    }

    private Type? GetRequestHandlerType(ref readonly RequestDetails requestDetails)
    {
        return requestDetails.CommandName switch
        {
            null => typeof(MockedRequestHandler),

            var cmd when IsSameCommand(cmd, DomainsCommandHandler.CommandName) => typeof(DomainsCommandHandler),
            var cmd when IsSameCommand(cmd, DomainConfigurationCommandHandler.CommandName) => typeof(DomainConfigurationCommandHandler),
            var cmd when IsSameCommand(cmd, UsageCountersCommandHandler.CommandName) => typeof(UsageCountersCommandHandler),

            _ => typeof(UnknownCommandHandler)
        };
    }

    private static bool IsSameCommand(string? requestedCommand, string expectedCommand) =>
        expectedCommand.Equals(requestedCommand, StringComparison.OrdinalIgnoreCase);
}