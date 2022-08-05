using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Infrastructure.RequestHandlers;
using HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHandlerRouter : IRequestHandlerRouter
    {
        private readonly IRequestDetailsProvider _requestDetailsProvider;

        public RequestHandlerRouter(IRequestDetailsProvider requestDetailsProvider)
        {
            _requestDetailsProvider = requestDetailsProvider;
        }

        public bool TryGetHandler(HttpContext httpContext, out RequestHandlerContext requestHandlerContext)
        {
            if (!_requestDetailsProvider.TryGetRequestDetails(httpContext, out var requestDetails))
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

        private bool TryGetManagementHandler(HttpContext httpContext, ref RequestDetails requestDetails, out IRequestHandler? result)
        {
            if (!IsCommandRequest(ref requestDetails, out var commandName))
            {
                result = null;
                return false;
            }

            result = requestDetails.HttpMethod switch
            {
                _ when HttpMethods.IsGet(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ConfigureCommandName)
                    => httpContext.RequestServices.GetRequiredService<ConfigureCommandGetHandler>(),

                _ when HttpMethods.IsPut(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ConfigureCommandName)
                    => httpContext.RequestServices.GetRequiredService<ConfigureCommandPutHandler>(),

                _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ResetCounterCommandName)
                    => httpContext.RequestServices.GetRequiredService<ResetCounterCommandHandler>(),

                _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ResetConfigurationCommandName)
                    => httpContext.RequestServices.GetRequiredService<ResetConfigurationCommandHandler>(),

                _ => null
            };

            return result != null;
        }

        private static bool IsCommandRequest(ref RequestDetails requestDetails, out string? commandName)
        {
            var commandHeader = requestDetails.GetHeaderValue(Constants.HeaderNames.ManagementCommandRequestHeader);
            if (string.IsNullOrWhiteSpace(commandHeader))
            {
                commandName = null;
                return false;
            }

            commandName = commandHeader;
            return true;
        }
    }
}