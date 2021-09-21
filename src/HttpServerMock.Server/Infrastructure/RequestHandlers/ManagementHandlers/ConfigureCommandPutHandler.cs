using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ConfigureCommandPutHandler : IRequestHandler
    {
        private readonly IRequestDefinitionStorage _requestDefinitionStorage;
        private readonly IRequestDefinitionReaderProvider _requestDefinitionReaderProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ConfigureCommandPutHandler> _logger;

        public ConfigureCommandPutHandler(
            IRequestDefinitionStorage requestDefinitionStorage,
            IRequestDefinitionReaderProvider requestDefinitionReaderProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ConfigureCommandPutHandler> logger)
        {
            _requestDefinitionStorage = requestDefinitionStorage;
            _requestDefinitionReaderProvider = requestDefinitionReaderProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            var result = ProcessPutCommand(requestDetails, cancellationToken);
            return Task.FromResult(result);
        }

        private IResponseDetails ProcessPutCommand(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestDetails.Content))
                return new Models.ResponseDetails
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

            cancellationToken.ThrowIfCancellationRequested();

            using var contentReader = new StringReader(requestDetails.Content);

            var requestDefinitionReader = _requestDefinitionReaderProvider.GetReader();

            var requestDefinitions = requestDefinitionReader.Read(contentReader, cancellationToken);

            _logger.LogInformation($"Setup configuration: {requestDefinitions.DefinitionItems.Count()} items");

            _requestDefinitionStorage.AddSet(requestDefinitions);

            _requestHistoryStorage.Clear();

            return new Models.ResponseDetails
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
    }
}
