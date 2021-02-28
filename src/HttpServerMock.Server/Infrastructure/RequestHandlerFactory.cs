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

        public async Task<RequestHandlerContext?> GetHandler(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var requestDetails = await _requestDetailsProvider.GetRequestDetails(cancellationToken).ConfigureAwait(false);

            var managementHandler = GetManagementHandler(httpContext, requestDetails);
            if (managementHandler != null)
            {
                return new RequestHandlerContext(requestDetails, managementHandler);
            }

            return new RequestHandlerContext(requestDetails, httpContext.RequestServices.GetService<MockedRequestHandler>());
        }

        private IRequestHandler? GetManagementHandler(HttpContext httpContext, IRequestDetails requestDetails)
        {
            if (!requestDetails.IsCommandRequest(out var commandName))
                return null;

            if (Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) && requestDetails.HttpMethod == HttpMethods.Get)
                return httpContext.RequestServices.GetService<ConfigureCommandGetHandler>();

            if (Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) && requestDetails.HttpMethod == HttpMethods.Put)
                return httpContext.RequestServices.GetService<ConfigureCommandPutHandler>();

            if (Constants.HeaderValues.ResetCounterCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) && requestDetails.HttpMethod == HttpMethods.Post)
                return httpContext.RequestServices.GetService<ResetCounterCommandHandler>();

            if (Constants.HeaderValues.ResetConfigurationCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) && requestDetails.HttpMethod == HttpMethods.Post)
                return httpContext.RequestServices.GetService<ResetConfigurationCommandHandler>();

            return null;
        }
}
}
