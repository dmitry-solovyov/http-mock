using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ResetStatisticsCommandHandler : IRequestDetailsHandler
    {
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetStatisticsCommandHandler> _logger;

        public ResetStatisticsCommandHandler(
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetStatisticsCommandHandler> logger)
        {
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ResetStatisticsCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Post;

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            _logger.LogInformation($"Reset statistics. Current number of history items {_requestHistoryStorage.CurrentItemsCount}");
            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails?)new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}
