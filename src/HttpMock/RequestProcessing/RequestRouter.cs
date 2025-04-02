using HttpMock.Extensions;
using HttpMock.Models;
using HttpMock.RequestHandlers.CommandRequestHandlers;
using Microsoft.AspNetCore.Http.Extensions;

namespace HttpMock.RequestProcessing;

public interface IRequestRouter
{
    ValueTask<bool> TryExecuteRequestHandler(HttpContext httpContext, CancellationToken cancellationToken = default);
}

public partial class RequestRouter : IRequestRouter
{
    private const string CommandNameHeader = "X-HttpMock-Command";

    public async ValueTask<bool> TryExecuteRequestHandler(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        if (TryGetCommandRouteDetails(httpContext, out var commandRouteDetails))
        {
            await commandRouteDetails.RequestHandler.Execute(commandRouteDetails.RequestDetails, httpContext.Response, cancellationToken).ConfigureAwait(false);
            return true;
        }

        if (TryGetRouteDetails(httpContext, out var mockedRouteDetails))
        {
            await mockedRouteDetails.RequestHandler.Execute(mockedRouteDetails.RequestDetails, httpContext.Response, cancellationToken).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    private static bool TryGetCommandRouteDetails(HttpContext httpContext, out CommandRouteDetails commandRouteDetails)
    {
        if (!TryGetCommandRequestDetails(httpContext, out var commandRequestDetails))
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
            var cmd when IsCommandName(cmd, ConfigurationCommandHandler.CommandName) => typeof(ConfigurationCommandHandler),
            var cmd when IsCommandName(cmd, UsageCountersCommandHandler.CommandName) => typeof(UsageCountersCommandHandler),

            _ => typeof(IUnknownCommandHandler)
        };
    }

    private static bool IsCommandName(ReadOnlySpan<char> requestedCommand, ReadOnlySpan<char> expectedCommand) =>
        expectedCommand.SequenceEqual(requestedCommand, CharComparer.OrdinalIgnoreCase);

    private static bool TryGetCommandRequestDetails(HttpContext httpContext, out CommandRequestDetails commandRequestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            commandRequestDetails = default;
            return false;
        }

        var commandName = request.Headers.GetValue(CommandNameHeader);
        if (string.IsNullOrEmpty(commandName))
        {
            commandRequestDetails = default;
            return false;
        }

        var httpMethodType = request.GetHttpMethodType();
        var contentType = request.GetNormalizedContentType();

        commandRequestDetails = new CommandRequestDetails(commandName, httpMethodType, contentType, request.Body);
        return true;
    }

    private static bool TryGetRouteDetails(HttpContext httpContext, out RouteDetails routeDetails)
    {
        if (!TryGetRequestDetails(httpContext, out var requestDetails))
        {
            routeDetails = default;
            return false;
        }

        var requestHandler = httpContext.RequestServices.GetRequiredService<IMockedRequestHandler>();
        routeDetails = new RouteDetails(requestDetails, requestHandler);
        return true;
    }

    private static bool TryGetRequestDetails(HttpContext httpContext, out RequestDetails requestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            requestDetails = default;
            return false;
        }

        var path = request.GetEncodedPathAndQuery();
        var httpMethodType = request.GetHttpMethodType();

        requestDetails = new RequestDetails(httpMethodType, path);
        return true;
    }
}