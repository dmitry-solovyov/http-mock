using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HttpServerMock.Server.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace HttpServerMock.Server.Infrastructure.RequestHandlers
{
    public class MockedRequestHandler : IRequestHandler
    {
        private readonly IRequestHistoryStorage _requestHistoryStorage;
        private readonly ILogger<MockedRequestHandler> _logger;

        public MockedRequestHandler(
            IRequestHistoryStorage requestHistoryStorage,
            ILogger<MockedRequestHandler> logger)
        {
            _requestHistoryStorage = requestHistoryStorage;
            _logger = logger;
        }

        public bool CanHandle(IRequestDetails requestDetails) => true;

        public async Task<IResponseDetails?> HandleResponse(IRequestDetails requestDetails)
        {
            var mockedRequestWithDefinition = _requestHistoryStorage.GetMockedRequestWithDefinition(requestDetails);

            return await ProcessRequestDefinition(requestDetails, mockedRequestWithDefinition, CancellationToken.None);
        }

        private async Task<IResponseDetails?> ProcessRequestDefinition(IRequestDetails requestDetails, MockedRequestDefinition mockedRequestWithDefinition, CancellationToken cancellationToken)
        {
            var requestDefinition = mockedRequestWithDefinition.RequestDefinition;
            if (requestDefinition == null)
                return null;

            var handled = false;

            var response = new ResponseDetails();

            handled |= FillContentType(requestDefinition, response);

            handled |= FillStatusCode(requestDefinition, response);

            handled |= await FillDelay(requestDefinition, cancellationToken);

            handled |= FillPayload(requestDetails, requestDefinition, response);

            handled |= FillHeaders(requestDefinition, response);

            if (handled)
            {
                _logger.LogInformation($"Handler description={requestDefinition.Description ?? "N/A"}, Request counter={mockedRequestWithDefinition.MockedRequest.Counter}");
            }

            return handled ? response : null;
        }

        private static bool FillContentType(RequestDefinitionItem requestDefinition, ResponseDetails response)
        {
            if (!string.IsNullOrWhiteSpace(requestDefinition.Then.ContentType))
            {
                response.ContentType = requestDefinition.Then.ContentType;
                return true;
            }

            response.ContentType = MediaTypeNames.Application.Json;
            return false;
        }

        private static bool FillStatusCode(
            RequestDefinitionItem requestDefinition, ResponseDetails response)
        {
            if (requestDefinition.Then.StatusCode <= 0)
                return false;

            response.StatusCode = requestDefinition.Then.StatusCode;
            return true;
        }

        private static async ValueTask<bool> FillDelay(
            RequestDefinitionItem requestDefinition, CancellationToken cancellationToken)
        {
            if (!requestDefinition.Then.Delay.HasValue || requestDefinition.Then.Delay.Value <= 0)
                return false;

            await Task.Delay(requestDefinition.Then.Delay.Value, cancellationToken).ConfigureAwait(false);
            return true;
        }

        private bool FillPayload(IRequestDetails requestDetails, RequestDefinitionItem requestDefinition, ResponseDetails response)
        {
            if (string.IsNullOrWhiteSpace(requestDefinition.Then.Payload))
                return false;

            var payload = requestDefinition.Then.Payload;
            if (!string.IsNullOrWhiteSpace(payload))
            {
                while (payload.Contains("@guid"))
                    payload = payload.Replace("@guid", Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);

                foreach (var urlVariable in requestDefinition.When.UrlVariables)
                    while (payload.Contains($"@{urlVariable}"))
                    {
                        var match = Regex.Match(requestDetails.Uri, requestDefinition.When.UrlRegexExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        payload = payload.Replace($"@{urlVariable}", match.Groups[urlVariable]?.Value);
                    }
            }

            response.ContentType = requestDefinition.Then.ContentType;
            response.Content = payload;
            return true;
        }

        private static bool FillHeaders(RequestDefinitionItem requestDefinition, ResponseDetails response)
        {
            if (requestDefinition.Then.Headers == null || requestDefinition.Then.Headers.Count == 0)
                return false;

            if (response.Headers == null)
                response.Headers = new Dictionary<string, string>();

            foreach (var thenHeader in requestDefinition.Then.Headers)
            {
                response.Headers[thenHeader.Key] = thenHeader.Value;
            }
            return true;
        }
    }
}
