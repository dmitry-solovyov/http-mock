using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Infrastructure.RequestHandlers;
using HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHandlerRouter : IRequestHandlerRouter
    {
        private readonly IRequestDetailsProvider _requestDetailsProvider;

        public RequestHandlerRouter(IRequestDetailsProvider requestDetailsProvider)
        {
            _requestDetailsProvider = requestDetailsProvider;
        }

        public RequestHandlerContext GetHandler(HttpContext httpContext)
        {
            var requestDetails = _requestDetailsProvider.GetRequestDetails(httpContext);

            // Try to find a management handler
            if (TryGetManagementHandler(httpContext, requestDetails, out var managementHandler) && managementHandler != null)
            {
                return new RequestHandlerContext(requestDetails, managementHandler);
            }

            // Otherwise initiate a mock handler
            var requestHandler = httpContext.RequestServices.GetRequiredService<MockedRequestHandler>();
            return new RequestHandlerContext(requestDetails, requestHandler);
        }

        private bool TryGetManagementHandler(HttpContext httpContext, IRequestDetails? requestDetails, out IRequestHandler? result)
        {
            if (requestDetails == null || !requestDetails.IsCommandRequest(out var commandName))
            {
                result = null;
                return false;
            }

            result = requestDetails.HttpMethod switch
            {
                _ when HttpMethods.IsGet(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ConfigureCommandName)
                    => httpContext.RequestServices.GetService<ConfigureCommandGetHandler>(),

                _ when HttpMethods.IsPut(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ConfigureCommandName)
                    => httpContext.RequestServices.GetService<ConfigureCommandPutHandler>(),

                _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ResetCounterCommandName)
                    => httpContext.RequestServices.GetService<ResetCounterCommandHandler>(),

                _ when HttpMethods.IsPost(requestDetails.HttpMethod) &&
                       StringComparer.OrdinalIgnoreCase.Equals(commandName, Constants.HeaderValues.ResetConfigurationCommandName)
                    => httpContext.RequestServices.GetService<ResetConfigurationCommandHandler>(),

                _ => null
            };

            return result != null;
        }
    }
}
