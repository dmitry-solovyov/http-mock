using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ResetConfigurationCommandHandler : IRequestHandler
    {
        private readonly IRequestDefinitionProvider _requestDefinitionProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetConfigurationCommandHandler> _logger;

        public ResetConfigurationCommandHandler(
            IRequestDefinitionProvider requestDefinitionProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetConfigurationCommandHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ResetConfigurationCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Post;

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            _logger.LogInformation($"Reset configuration. Current number of definition items {_requestDefinitionProvider.Count}");
            _requestDefinitionProvider.Clear();
            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails?)new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}
