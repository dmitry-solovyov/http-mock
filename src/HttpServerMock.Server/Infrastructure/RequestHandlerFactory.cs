using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Infrastructure.RequestHandlers;
using HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure
{
    public class RequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly IRequestDetailsProvider _requestDetailsProvider;

        public RequestHandlerFactory(IRequestDetailsProvider requestDetailsProvider)
        {
            _requestDetailsProvider = requestDetailsProvider;
        }

        public async Task<RequestHandlerContext> GetHandlerContext(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var requestDetails = await _requestDetailsProvider.GetRequestDetails(cancellationToken).ConfigureAwait(false);

            // Try to find a management handler
            var managementHandler = GetManagementHandler(httpContext, requestDetails);

            // Use found management handler, otherwise initiate a mock handler
            var requestHandler = managementHandler ?? httpContext.RequestServices.GetService<MockedRequestHandler>();

            return new RequestHandlerContext(requestDetails, requestHandler);
        }

        private IRequestHandler? GetManagementHandler(HttpContext httpContext, IRequestDetails requestDetails)
        {
            if (!requestDetails.IsCommandRequest(out var commandName))
                return null;

            IRequestHandler? result = requestDetails.HttpMethod switch
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

            return result;
        }
    }
}
