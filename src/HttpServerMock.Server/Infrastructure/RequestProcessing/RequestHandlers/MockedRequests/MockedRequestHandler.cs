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

        HttpResponseDetails response;

        if (IsProxyRequest(ref requestDefinition, out var parsedUrl))
        {
            response = await ProcessProxyRequest(parsedUrl!, requestDefinition, cancellationToken);
        }
        else
        {
            response = ProcessMockedRequest(ref requestDetails, ref requestDefinition);
        }

        if (requestDefinition.Then.Delay > 0)
        {
            await Task.Delay(requestDefinition.Then.Delay.Value, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation($"Handler description={requestDefinition.Description ?? "N/A"}, Request counter={mockedRequestWithDefinition.MockedRequest.Counter}");

        return response;
    }

    private bool IsProxyRequest(ref ConfigurationStorageItem requestDefinition, out Uri? parsedUrl)
    {
        parsedUrl = null;
        return !string.IsNullOrEmpty(requestDefinition.Then.ProxyUrl) && Uri.TryCreate(requestDefinition.Then.ProxyUrl, UriKind.Absolute, out parsedUrl);
    }

    private async Task<HttpResponseDetails> ProcessProxyRequest(Uri proxyUrl, ConfigurationStorageItem requestDefinition, CancellationToken cancellationToken)
    {
        var response = new HttpResponseDetails();

        try
        {
            var httpMethod = new HttpMethod(requestDefinition.Then.Method!);

            var httpRequestMessage = new HttpRequestMessage(
                httpMethod,
                proxyUrl);

            if (requestDefinition.Then.Headers != null)
                foreach (var thenHeader in requestDefinition.Then.Headers)
                {
                    httpRequestMessage.Headers.Add(thenHeader.Key, thenHeader.Value);
                }

            response.ContentType = requestDefinition.Then.ContentType;

            using var httpClient = new HttpClient();
            var httpResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

            response.Content = await httpResponse.Content.ReadAsStringAsync();
            response.Headers = httpResponse.Headers.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault() ?? string.Empty);
            response.StatusCode = (int)httpResponse.StatusCode;
        }
        catch (HttpRequestException hex)
        {
            response.StatusCode = hex.StatusCode.HasValue ? (int)hex.StatusCode : 0;
            response.Content = hex.Message;
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Content = ex.Message;
        }

        return response;
    }

    private HttpResponseDetails ProcessMockedRequest(ref HttpRequestDetails requestDetails, ref ConfigurationStorageItem requestDefinition)
    {
        var response = new HttpResponseDetails();

        FillContentType(ref requestDefinition, ref response);

        FillStatusCode(ref requestDefinition, ref response);

        FillDelay(ref requestDefinition, ref response);

        FillPayload(ref requestDetails, ref requestDefinition, ref response);

        FillHeaders(ref requestDefinition, ref response);

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