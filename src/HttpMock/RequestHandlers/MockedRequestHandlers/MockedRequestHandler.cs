using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestHandler : IRequestHandler
{
    private const int MaxDelay = 60_000;

    private readonly IConfigurationStorage _configurationStorage;
    private readonly IMockedRequestEndpointConfigurationResolver _mockedRequestEndpointConfigurationResolver;

    public MockedRequestHandler(
        IConfigurationStorage configurationStorage,
        IMockedRequestEndpointConfigurationResolver mockedRequestEndpointConfigurationResolver)
    {
        _configurationStorage = configurationStorage;
        _mockedRequestEndpointConfigurationResolver = mockedRequestEndpointConfigurationResolver;
    }

    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(requestDetails.Domain))
        {
            httpResponse.WithStatusCode(StatusCodes.Status400BadRequest);
            return;
        }

        if (_mockedRequestEndpointConfigurationResolver.TryGetEndpointConfiguration(ref requestDetails, out var endpointConfiguration))
        {
            await ApplyEndpointConfiguration(requestDetails, httpResponse, endpointConfiguration!, cancellationToken).ConfigureAwait(false);
            return;
        }

        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownMockedResponse);
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

            await httpResponse
                .WithStatusCode((int)httpProxyResponse.StatusCode)
                .WithHeaders(httpProxyResponse.Headers.ToDictionary(x => x.Key, x => x.Value?.ToString()))
                .WithContentAsync(httpProxyResponse.Content.ReadAsStream(), endpointConfiguration.Then.ContentType)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException hex)
        {
            httpResponse
                .WithStatusCode(hex.StatusCode.HasValue ? (int)hex.StatusCode : 0)
                .WithContent(hex.Message, Defaults.ContentTypes.ContentTypeForUntypedResponse);
        }
        catch (Exception ex)
        {
            httpResponse
                .WithStatusCode(StatusCodes.Status500InternalServerError)
                .WithContent(ex.Message, Defaults.ContentTypes.ContentTypeForUntypedResponse);
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

        httpResponse.WithStatusCode(StatusCodes.Status200OK);
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
        catch
        {
            ;
        }
    }

    private async ValueTask ProcessMockedRequest(string queryPath, EndpointConfiguration endpointConfiguration, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        var delay = GetDelay(endpointConfiguration);
        if (delay > 0)
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        var statusCode = GetStatusCode(endpointConfiguration);

        var headers = GetHeaders(endpointConfiguration);

        var contentType = GetContentType(endpointConfiguration);
        var content = GetPayload(queryPath, endpointConfiguration);

        await httpResponse
            .WithStatusCode(statusCode)
            .WithHeaders(headers)
            .WithContentAsync(content, contentType, cancellationToken).ConfigureAwait(false);
    }

    private static int GetDelay(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.Delay switch
        {
            > 0 and <= MaxDelay => endpointConfiguration.Then.Delay,
            > MaxDelay => MaxDelay,
            _ => 0
        };

    private static string GetContentType(EndpointConfiguration endpointConfiguration) =>
        !string.IsNullOrWhiteSpace(endpointConfiguration.Then.ContentType) ? endpointConfiguration.Then.ContentType : Defaults.ContentTypes.ContentTypeForMockedResponse;

    private static int GetStatusCode(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.StatusCode is >= 100 and <= 599 ? endpointConfiguration.Then.StatusCode : Defaults.StatusCodes.StatusCodeForUnknownMockedResponse;

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

    private static Dictionary<string, string?>? GetHeaders(EndpointConfiguration endpointConfiguration)
    {
        if (endpointConfiguration.Then.Headers == default || endpointConfiguration.Then.Headers.Count == 0)
            return default;

        var headers = new Dictionary<string, string?>();
        foreach (var thenHeader in endpointConfiguration.Then.Headers)
        {
            headers[thenHeader.Key] = thenHeader.Value;
        }
        return headers;
    }
}