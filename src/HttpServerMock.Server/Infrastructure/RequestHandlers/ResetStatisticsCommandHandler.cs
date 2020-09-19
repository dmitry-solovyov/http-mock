using HttpServerMock.Server.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class ResetStatisticsCommandHandler : IRequestDetailsHandler
    {
        private readonly IRequestHistoryStorage _requestHistoryStorage;

        public ResetStatisticsCommandHandler(
            IRequestHistoryStorage requestHistoryStorage)
        {
            _requestHistoryStorage = requestHistoryStorage;
        }

        public bool CanHandle(IRequestDetails requestDetails) =>
            requestDetails.IsCommandRequest(out var commandName) &&
            Constants.HeaderValues.ResetStatisticsCommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase) &&
            requestDetails.HttpMethod == HttpMethods.Put;

        public Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            Console.WriteLine("| Reset statistics");
            _requestHistoryStorage.Clear();

            return Task.FromResult((IResponseDetails?)new ResponseDetails
            {
                StatusCode = StatusCodes.Status200OK
            });
        }
    }
}
