using HttpServerMock.Server.Infrastructure.Extensions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ResetCounterCommandHandler : IRequestHandler
    {
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<ResetCounterCommandHandler> _logger;

        public ResetCounterCommandHandler(
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<ResetCounterCommandHandler> logger)
        {
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ResetCounterCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Post;

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            _logger.LogInformation($"Reset counters. Current number of history items {_requestHistoryStorage.CurrentItemsCount}");
            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails?)new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}
