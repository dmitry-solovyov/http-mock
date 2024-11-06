using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestHandler : IRequestHandler
{
    private const int MaxDelay = 60_000;
    private const int DefaultStatusCode = StatusCodes.Status200OK;
    private const string DefaultContentType = MediaTypeNames.Application.Json;

    private readonly ILogger<MockedRequestHandler> _logger;
    private readonly IConfigurationStorage _configurationStorage;
    private readonly IMockedRequestEndpointConfigurationResolver _mockedRequestEndpointConfigurationResolver;

    public MockedRequestHandler(
        ILogger<MockedRequestHandler> logger,
        IConfigurationStorage configurationStorage,
        IMockedRequestEndpointConfigurationResolver mockedRequestEndpointConfigurationResolver)
    {
        _logger = logger;
        _configurationStorage = configurationStorage;
        _mockedRequestEndpointConfigurationResolver = mockedRequestEndpointConfigurationResolver;
    }

    public async ValueTask Execute(RequestDetails requestDetails, HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(requestDetails.Domain) || !_configurationStorage.IsDomainExists(requestDetails.Domain))
        {
            await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (_mockedRequestEndpointConfigurationResolver.TryGetEndpointConfiguration(ref requestDetails, out var endpointConfiguration))
        {
            await ApplyEndpointConfiguration(requestDetails, httpResponse, endpointConfiguration!, cancellationToken).ConfigureAwait(false);
            return;
        }

        await httpResponse.FillContentAsync(StatusCodes.Status404NotFound, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask ApplyEndpointConfiguration(RequestDetails requestDetails, HttpResponse httpResponse, EndpointConfiguration endpointConfiguration, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(endpointConfiguration.Then.ProxyUrl))
        {
            await ProcessProxyRequest(endpointConfiguration.Then.ProxyUrl, endpointConfiguration, httpResponse, cancellationToken).ConfigureAwait(false);
        }
        else if (!string.IsNullOrEmpty(endpointConfiguration.Then.CallbackUrl))
        {
            ProcessCallbackRequest(endpointConfiguration.Then.CallbackUrl, endpointConfiguration, httpResponse);
        }
        else
        {
            await ProcessMockedRequest(requestDetails.QueryPath, endpointConfiguration, httpResponse, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug($"Endpoint description: {endpointConfiguration.Description ?? "N/A"}");
    }

    private async Task ProcessProxyRequest(string proxyUrl, EndpointConfiguration endpointConfiguration, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        try
        {
            var delay = GetDelay(endpointConfiguration);
            if (delay > 0)
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            var httpMethod = new HttpMethod(endpointConfiguration.When.HttpMethod.ToString());

            var httpProxyRequestMessage = new HttpRequestMessage(httpMethod, proxyUrl);

            if (endpointConfiguration.Then.Headers != default)
                foreach (var header in endpointConfiguration.Then.Headers)
                {
                    httpProxyRequestMessage.Headers.Add(header.Key, header.Value);
                }

            //TODO: review the http client builder
            using var httpClient = new HttpClient();
            var httpProxyResponse = await httpClient.SendAsync(httpProxyRequestMessage, cancellationToken).ConfigureAwait(false);

            await httpResponse.FillContentAsync((int)httpProxyResponse.StatusCode, httpProxyResponse.Content.ReadAsStream(), endpointConfiguration.Then.ContentType).ConfigureAwait(false);
            httpResponse.FillHeaders(httpProxyResponse.Headers.ToDictionary(x => x.Key, x => string.Join(',', x.Value)));
        }
        catch (HttpRequestException hex)
        {
            httpResponse.FillContent(hex.StatusCode.HasValue ? (int)hex.StatusCode : 0, content: hex.Message);
        }
        catch (Exception ex)
        {
            httpResponse.FillContent(StatusCodes.Status500InternalServerError, content: ex.Message);
        }
    }

    private void ProcessCallbackRequest(string callbackUrl, EndpointConfiguration endpointConfiguration, HttpResponse httpResponse)
    {
        EndpointConfiguration endpointConfigurationLocal = endpointConfiguration;

        var callbackUri = new Uri(callbackUrl);

        var task = Task.Factory.StartNew(
            () => PushHttpData(callbackUri, endpointConfigurationLocal, CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        ).ConfigureAwait(false);

        httpResponse.StatusCode = StatusCodes.Status200OK;
    }

    private async Task PushHttpData(Uri url, EndpointConfiguration endpointConfiguration, CancellationToken cancellationToken)
    {
        try
        {
            var delay = GetDelay(endpointConfiguration);
            if (delay > 0)
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            var httpMethod = new HttpMethod(endpointConfiguration.When.HttpMethod.ToString());

            var httpRequestMessage = new HttpRequestMessage(httpMethod, url);

            var headers = GetHeaders(endpointConfiguration);
            if (headers != default)
                foreach (var header in headers)
                    httpRequestMessage.Headers.Add(header.Key, header.Value);

            var contentType = GetContentType(endpointConfiguration);

            if (!string.IsNullOrEmpty(endpointConfiguration.Then.Payload))
                httpRequestMessage.Content = new StringContent(endpointConfiguration.Then.Payload, Encoding.UTF8, contentType);

            //TODO: review the http client builder
            using var httpClient = new HttpClient();
            var _ = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException hex)
        {
            _logger.LogError("Failed on pushing HTTP data with error {Message}", hex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed on pushing HTTP data with error {Message}", ex.Message);
        }
    }

    private async ValueTask ProcessMockedRequest(string queryPath, EndpointConfiguration endpointConfiguration, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        var delay = GetDelay(endpointConfiguration);
        if (delay > 0)
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        var headers = GetHeaders(endpointConfiguration);
        httpResponse.FillHeaders(headers);

        var contentType = GetContentType(endpointConfiguration);
        var statusCode = GetStatusCode(endpointConfiguration);
        var content = GetPayload(queryPath, endpointConfiguration);

        await httpResponse.FillContentAsync(statusCode, content, contentType, cancellationToken).ConfigureAwait(false);
    }

    private static int GetDelay(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.Delay switch
        {
            > 0 and <= MaxDelay => endpointConfiguration.Then.Delay,
            > MaxDelay => MaxDelay,
            _ => 0
        };

    private static string GetContentType(EndpointConfiguration endpointConfiguration) =>
        !string.IsNullOrWhiteSpace(endpointConfiguration.Then.ContentType) ? endpointConfiguration.Then.ContentType : DefaultContentType;

    private static int GetStatusCode(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.StatusCode is >= 100 and <= 599 ? endpointConfiguration.Then.StatusCode : DefaultStatusCode;

    private string? GetPayload(string queryPath, EndpointConfiguration endpointConfiguration)
    {
        var payload = endpointConfiguration.Then.Payload;
        if (!string.IsNullOrWhiteSpace(payload))
        {
            payload = ReplaceGuids(payload);

            if (endpointConfiguration.When.UrlRegexExpression != default && endpointConfiguration.When.UrlVariables != default)
                foreach (var urlVariable in endpointConfiguration.When.UrlVariables)
                {
                    var regexVarName = $"@{urlVariable}";
                    while (payload.Contains(regexVarName))
                    {
                        var match = Regex.Match(queryPath, endpointConfiguration.When.UrlRegexExpression, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        payload = payload.Replace(regexVarName, match.Groups[urlVariable]?.Value);
                    }
                }
        }
        return payload;
    }

    private static string ReplaceGuids(string payload)
    {
        const string guidVarName = "@guid";

        var comparison = StringComparison.OrdinalIgnoreCase;

        while (payload.IndexOf(guidVarName, comparison) != -1)
            payload = payload.Replace(guidVarName, Guid.NewGuid().ToString(), comparison);

        return payload;
    }

    private static Dictionary<string, string>? GetHeaders(EndpointConfiguration endpointConfiguration)
    {
        if (endpointConfiguration.Then.Headers == default || endpointConfiguration.Then.Headers.Count == 0)
            return default;

        var headers = new Dictionary<string, string>();
        foreach (var thenHeader in endpointConfiguration.Then.Headers)
        {
            headers[thenHeader.Key] = thenHeader.Value;
        }
        return headers;
    }
}