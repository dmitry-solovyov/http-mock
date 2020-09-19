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

        public ConfigureCommandPutHandler(
            IRequestDefinitionProvider requestDefinitionProvider,
            IRequestDefinitionReader requestDefinitionReader)
        {
            _requestDefinitionProvider = requestDefinitionProvider;
            _requestDefinitionReader = requestDefinitionReader;
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
            var requestDefinitions = _requestDefinitionReader.Read(new StringReader(requestDetails.Content));

            _requestDefinitionProvider.AddRange(requestDefinitions);

            return new ResponseDetails
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}
