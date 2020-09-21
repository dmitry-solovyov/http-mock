using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ConfigureCommandPutHandler : IRequestDetailsHandler
    {
        private readonly IRequestDefinitionProvider _requestDefinitionProvider;
        private readonly IRequestDefinitionReader _requestDefinitionReader;
        private readonly IRequestHistoryStorage _requestHistoryStorage;

        public ConfigureCommandPutHandler(
            IRequestDefinitionProvider requestDefinitionProvider,
            IRequestDefinitionReader requestDefinitionReader,
            IRequestHistoryStorage requestHistoryStorage)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestDefinitionReader = requestDefinitionReader;
            _requestHistoryStorage = requestHistoryStorage;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ConfigureCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Put &&
            !string.IsNullOrWhiteSpace(requestDetails.Content);

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            Console.WriteLine("| Set configuration");
            return Task.FromResult(ProcessPutCommand(requestDetails));
        }

        private IResponseDetails? ProcessPutCommand(IRequestDetails requestDetails)
        {
            if (string.IsNullOrWhiteSpace(requestDetails.Content))
                return new ResponseDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

            var requestDefinitions = _requestDefinitionReader.Read(new StringReader(requestDetails.Content));

            _requestDefinitionProvider.AddRange(requestDefinitions);

            _requestHistoryStorage.Clear();

            return new ResponseDetails
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}
