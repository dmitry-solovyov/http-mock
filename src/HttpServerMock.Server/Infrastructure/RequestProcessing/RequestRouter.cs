using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.ManagementRequests;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing;

public class RequestRouter : IRequestRouter
{
    private readonly IHttpRequestDetailsProvider _htpRequestDetailsProvider;

    public RequestRouter(IHttpRequestDetailsProvider httpRequestDetailsProvider)
    {
        _htpRequestDetailsProvider = httpRequestDetailsProvider;
    }

    public bool TryGetHandlerContext(HttpContext httpContext, out RequestHandlerContext requestHandlerContext)
    {
        if (!_htpRequestDetailsProvider.TryGetRequestDetails(httpContext, out var requestDetails))
        {
            requestHandlerContext = default;
            return false;
        }

        // Try to find a management handler
        if (TryGetManagementHandler(httpContext, ref requestDetails, out var managementHandler) && managementHandler != null)
        {
            requestHandlerContext = new RequestHandlerContext(requestDetails, managementHandler);
            return true;
        }

        // Otherwise initiate a mock handler
        var requestHandler = httpContext.RequestServices.GetRequiredService<MockedRequestHandler>();
        requestHandlerContext = new RequestHandlerContext(requestDetails, requestHandler);
        return true;
    }

    private bool TryGetManagementHandler(HttpContext httpContext, ref HttpRequestDetails requestDetails, out IRequestHandler? managementHandler)
    {
        if (!IsManagementRequest(ref requestDetails, out var managementCommandName))
        {
            managementHandler = null;
            return false;
        }

        managementHandler = requestDetails.HttpMethod switch
        {
            _ when HttpMethods.IsGet(requestDetails.HttpMethod) &&
                   StringComparer.OrdinalIgnoreCase.Equals(managementCommandName, Constants.HeaderValues.ConfigureCommandName)
                => httpContext.RequestServices.GetRequiredService<ConfigureCommandGetHandler>(),

            _ when HttpMethods.IsPut(requestDetails.HttpMethod) &&
                   StringComparer.OrdinalIgnoreCase.Equals(managementCommandName, Constants.HeaderValues.ConfigureCommandName)
                => httpContext.RequestServices.GetRequiredService<ConfigureCommandPutHandler>(),

            _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                   StringComparer.OrdinalIgnoreCase.Equals(managementCommandName, Constants.HeaderValues.ResetCounterCommandName)
                => httpContext.RequestServices.GetRequiredService<ResetCounterCommandHandler>(),

            _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                   StringComparer.OrdinalIgnoreCase.Equals(managementCommandName, Constants.HeaderValues.ResetConfigurationCommandName)
                => httpContext.RequestServices.GetRequiredService<ResetConfigurationCommandHandler>(),

            _ => null
        };

        return managementHandler != null;
    }

    private static bool IsManagementRequest(ref HttpRequestDetails requestDetails, out string? managementCommandName)
    {
        var managementCommandHeader = requestDetails.GetHeaderValue(Constants.HeaderNames.ManagementCommandRequestHeader);

        managementCommandName = managementCommandHeader;

        return string.IsNullOrWhiteSpace(managementCommandHeader) != true;
    }
}