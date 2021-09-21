﻿using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ResetConfigurationCommandHandler : IRequestHandler
    {
        private readonly IRequestDefinitionStorage _requestDefinitionProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetConfigurationCommandHandler> _logger;

        public ResetConfigurationCommandHandler(
            IRequestDefinitionStorage requestDefinitionProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetConfigurationCommandHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Reset configuration. Current number of definition items {_requestDefinitionProvider.GetCount()}");

            _requestDefinitionProvider.Clear();
            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails)new Models.ResponseDetails { StatusCode = StatusCodes.Status200OK });
        }
    }
}
