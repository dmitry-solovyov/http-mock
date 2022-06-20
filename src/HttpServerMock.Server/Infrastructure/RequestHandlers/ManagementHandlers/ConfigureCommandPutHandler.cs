using HttpServerMock.RequestDefinitions;
using HttpServerMock.RequestDefinitions.Converters;
using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers.ManagementHandlers
{
    public class ConfigureCommandPutHandler : IRequestHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestDefinitionStorage _requestDefinitionStorage;
        private readonly IRequestDefinitionReaderProvider _requestDefinitionReaderProvider;
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ConfigureCommandPutHandler> _logger;

        public ConfigureCommandPutHandler(
            IHttpContextAccessor httpContextAccessor,
            IRequestDefinitionStorage requestDefinitionStorage,
            IRequestDefinitionReaderProvider requestDefinitionReaderProvider,
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ConfigureCommandPutHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestDefinitionStorage = requestDefinitionStorage;
            _requestDefinitionReaderProvider = requestDefinitionReaderProvider;
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public Task<IResponseDetails> Execute(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            return ProcessPutCommand(requestDetails, cancellationToken);
        }

        private async Task<IResponseDetails> ProcessPutCommand(IRequestDetails requestDetails, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_httpContextAccessor.HttpContext == null)
                return new Models.ResponseDetails { StatusCode = StatusCodes.Status400BadRequest };

            var requestDefinitionReader = _requestDefinitionReaderProvider.GetReader();

            var content = await _httpContextAccessor.HttpContext.Request.GetContent(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
                return new Models.ResponseDetails { StatusCode = StatusCodes.Status400BadRequest };

            var configurationDefinitions = requestDefinitionReader.Read(content);

            var requestDefinitions = ConfigurationDefinitionConverter.ToDefinitionSet(configurationDefinitions);
            if (requestDefinitions == null)
            {
                return new Models.ResponseDetails { StatusCode = StatusCodes.Status400BadRequest };
            }

            _logger.LogInformation($"Setup configuration: {requestDefinitions.DefinitionItems.Count()} items");

            _requestDefinitionStorage.AddSet(requestDefinitions);

            _requestHistoryStorage.Clear();

            return new Models.ResponseDetails { StatusCode = StatusCodes.Status202Accepted };
        }
    }
}
