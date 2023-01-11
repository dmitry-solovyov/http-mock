using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;

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

    public async ValueTask<HttpResponseDetails> Execute(HttpRequestDetails requestDetails, CancellationToken cancellationToken = default)
    {
        var mockedRequestDefinition = _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);

        return await ProcessRequestDefinition(requestDetails, mockedRequestDefinition, cancellationToken);
    }

    private async ValueTask<HttpResponseDetails> ProcessRequestDefinition(HttpRequestDetails requestDetails, MockedRequestDefinition mockedRequestWithDefinition, CancellationToken cancellationToken = default)
    {
        var requestDefinition = mockedRequestWithDefinition.RequestDefinition;
        if (requestDefinition == null)
            return new HttpResponseDetails { StatusCode = StatusCodes.Status200OK };

        var handled = false;

        var response = new HttpResponseDetails();

        handled |= FillContentType(ref requestDefinition, ref response);

        handled |= FillStatusCode(ref requestDefinition, ref response);

        handled |= FillDelay(ref requestDefinition, ref response);

        handled |= FillPayload(ref requestDetails, ref requestDefinition, ref response);

        handled |= FillHeaders(ref requestDefinition, ref response);

        if (requestDefinition.Then.Delay > 0)
        {
            await Task.Delay(requestDefinition.Then.Delay.Value, cancellationToken).ConfigureAwait(false);
        }

        if (handled)
        {
            _logger.LogInformation($"Handler description={requestDefinition.Description ?? "N/A"}, Request counter={mockedRequestWithDefinition.MockedRequest.Counter}");
        }

        return response;
    }

    private static bool FillContentType(
        ref ConfigurationStorageItem requestDefinition, ref HttpResponseDetails response)
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
        ref ConfigurationStorageItem requestDefinition, ref HttpResponseDetails response)
    {
        if (requestDefinition.Then.StatusCode <= 0)
            return false;

        response.StatusCode = requestDefinition.Then.StatusCode;
        return true;
    }

    private static bool FillDelay(ref ConfigurationStorageItem requestDefinition, ref HttpResponseDetails response)
    {
        if (!requestDefinition.Then.Delay.HasValue || requestDefinition.Then.Delay.Value <= 0)
            return false;

        response.Delay = requestDefinition.Then.Delay.Value;
        return true;
    }

    private bool FillPayload(
        ref HttpRequestDetails requestDetails, ref ConfigurationStorageItem requestDefinition, ref HttpResponseDetails response)
    {
        var payload = requestDefinition.Then.Payload;

        if (string.IsNullOrWhiteSpace(payload))
            return false;

        payload = ReplaceGuids(payload);

        if (requestDefinition.When.UrlRegexExpression != null)
            foreach (var urlVariable in requestDefinition.When.UrlVariables)
            {
                var regexVarName = $"@{urlVariable}";
                while (payload.Contains(regexVarName))
                {
                    var match = Regex.Match(requestDetails.Url, requestDefinition.When.UrlRegexExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    payload = payload.Replace(regexVarName, match.Groups[urlVariable]?.Value);
                }
            }

        response.ContentType = requestDefinition.Then.ContentType;
        response.Content = payload;
        return true;
    }

    private static bool FillHeaders(
        ref ConfigurationStorageItem requestDefinition, ref HttpResponseDetails response)
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
        const string guidVarName = "@guid";

        while (payload.Contains(guidVarName))
            payload = payload.Replace(guidVarName, Guid.NewGuid().ToString(), StringComparison.OrdinalIgnoreCase);

        return payload;
    }
}