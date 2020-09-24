using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ConfigureCommandPutHandler : IRequestDetailsHandler
    {
        private readonly IRequestDefinitionProvider _requestDefinitionProvider;
        private readonly IRequestDefinitionReader _requestDefinitionReader;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ConfigureCommandPutHandler> _logger;

        public ConfigureCommandPutHandler(
            IRequestDefinitionProvider requestDefinitionProvider,
            IRequestDefinitionReader requestDefinitionReader,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ConfigureCommandPutHandler> logger)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestDefinitionReader = requestDefinitionReader;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Put &&
            !string.IsNullOrWhiteSpace(requestDetails.Content);

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            var result = ProcessPutCommand(requestDetails);
            return Task.FromResult(result);
        }

        private IResponseDetails? ProcessPutCommand(IRequestDetails requestDetails)
        {
            if (string.IsNullOrWhiteSpace(requestDetails.Content))
                return new ResponseDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

            var requestDefinitions = _requestDefinitionReader.Read(new StringReader(requestDetails.Content));

            _logger.LogInformation($"Setup configuration: {requestDefinitions.Definitions.Count()}");

            _requestDefinitionProvider.AddRange(requestDefinitions);

            _requestHistoryStorage.Clear();

            return new ResponseDetails
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}
