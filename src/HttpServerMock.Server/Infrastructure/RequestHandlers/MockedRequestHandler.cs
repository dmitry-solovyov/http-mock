using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure.Interfaces;
using HttpServerMock.Server.Models;
using System.Net.Mime;
using System.Text.RegularExpressions;

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

        public ValueTask<Models.ResponseDetails> Execute(RequestDetails requestDetails, CancellationToken cancellationToken = default)
        {
            var mockedRequestDefinition = _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);

            return ValueTask.FromResult(ProcessRequestDefinition(requestDetails, mockedRequestDefinition));
        }

        private Models.ResponseDetails ProcessRequestDefinition(RequestDetails requestDetails, MockedRequestDefinition mockedRequestWithDefinition)
        {
            var requestDefinition = mockedRequestWithDefinition.RequestDefinition;
            if (requestDefinition == null)
                return new Models.ResponseDetails { StatusCode = StatusCodes.Status200OK };

            var handled = false;

            var response = new Models.ResponseDetails();

            handled |= FillContentType(ref requestDefinition, ref response);

            handled |= FillStatusCode(ref requestDefinition, ref response);

            handled |= FillDelay(ref requestDefinition, ref response);

            handled |= FillPayload(ref requestDetails, ref requestDefinition, ref response);

            handled |= FillHeaders(ref requestDefinition, ref response);

            if (handled)
            {
                _logger.LogInformation($"Handler description={requestDefinition.Description ?? "N/A"}, Request counter={mockedRequestWithDefinition.MockedRequest.Counter}");
            }

            return response;
        }

        private static bool FillContentType(
            ref RequestDefinitionItem requestDefinition, ref Models.ResponseDetails response)
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
            ref RequestDefinitionItem requestDefinition, ref Models.ResponseDetails response)
        {
            if (requestDefinition.Then.StatusCode <= 0)
                return false;

            response.StatusCode = requestDefinition.Then.StatusCode;
            return true;
        }

        private static bool FillDelay(ref RequestDefinitionItem requestDefinition, ref Models.ResponseDetails response)
        {
            if (!requestDefinition.Then.Delay.HasValue || requestDefinition.Then.Delay.Value <= 0)
                return false;

            response.Delay = requestDefinition.Then.Delay.Value;
            return true;
        }

        private bool FillPayload(
            ref RequestDetails requestDetails, ref RequestDefinitionItem requestDefinition, ref Models.ResponseDetails response)
        {
            var payload = requestDefinition.Then.Payload;

            if (string.IsNullOrWhiteSpace(payload))
                return false;

            payload = ReplaceGuids(payload);

            if (requestDefinition.When.UrlRegexExpression != null)
                foreach (var urlVariable in requestDefinition.When.UrlVariables)
                    while (payload.Contains($"@{urlVariable}"))
                    {
                        var match = Regex.Match(requestDetails.Url, requestDefinition.When.UrlRegexExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        payload = payload.Replace($"@{urlVariable}", match.Groups[urlVariable]?.Value);
                    }

            response.ContentType = requestDefinition.Then.ContentType;
            response.Content = payload;
            return true;
        }

        private static bool FillHeaders(
            ref RequestDefinitionItem requestDefinition, ref Models.ResponseDetails response)
        {
            if (requestDefinition.Then.Headers == null || requestDefinition.Then.Headers.Count == 0)
                return false;

            response.Headers ??= new Dictionary<string, string>();

            foreach (var thenHeader in requestDefinition.Then.Headers)
            {
                response.Headers[thenHeader.Key] = thenHeader.Value;
            }
            return true;
        }

        private static string ReplaceGuids(string payload)
        {
            while (payload.Contains("@guid"))
                payload = payload.Replace("@guid", Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);

            return payload;
        }
    }
}