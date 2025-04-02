using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text;
using static HttpMock.RequestHandlers.MockedRequestHandlers.MockedRequestEndpointConfigurationResolver;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestHandler : IMockedRequestHandler
{
    private const int MaxDelay = 60_000;

    private readonly IMockedRequestEndpointConfigurationResolver _mockedRequestEndpointConfigurationResolver;

    public MockedRequestHandler(IMockedRequestEndpointConfigurationResolver mockedRequestEndpointConfigurationResolver)
    {
        _mockedRequestEndpointConfigurationResolver = mockedRequestEndpointConfigurationResolver;
    }

    public async ValueTask Execute(RequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (_mockedRequestEndpointConfigurationResolver.TryGetEndpointConfiguration(ref requestDetails, out var endpointConfiguration, out var foundVariables))
        {
            await ApplyEndpointConfiguration(requestDetails, httpResponse, endpointConfiguration!, foundVariables, cancellationToken).ConfigureAwait(false);
            return;
        }

        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownMockedResponse);
    }

    private static async ValueTask ApplyEndpointConfiguration(RequestDetails requestDetails, HttpResponse httpResponse, EndpointConfiguration endpointConfiguration, List<PathVariable>? foundVariables, CancellationToken cancellationToken = default)
    {
        var delay = GetDelay(endpointConfiguration);
        if (delay > 0)
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        var statusCode = GetStatusCode(endpointConfiguration);
        var contentType = GetContentType(endpointConfiguration);
        var content = GetPayload(requestDetails, endpointConfiguration, foundVariables);

        await httpResponse
            .WithStatusCode(statusCode)
            .WithHeaders(endpointConfiguration.Then.Headers)
            .WithContentAsync(content, contentType, cancellationToken).ConfigureAwait(false);
    }

    private static ushort GetDelay(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.Delay switch
        {
            > 0 and <= MaxDelay => endpointConfiguration.Then.Delay,
            > MaxDelay => MaxDelay,
            _ => 0
        };

    private static string GetContentType(EndpointConfiguration endpointConfiguration) =>
        !string.IsNullOrWhiteSpace(endpointConfiguration.Then.ContentType) ? endpointConfiguration.Then.ContentType : Defaults.ContentTypes.ContentTypeForMockedResponse;

    private static ushort GetStatusCode(EndpointConfiguration endpointConfiguration) =>
        endpointConfiguration.Then.StatusCode is >= 100 and <= 599 ? endpointConfiguration.Then.StatusCode : Defaults.StatusCodes.StatusCodeForUnknownMockedResponse;

    private static string? GetPayload(RequestDetails requestDetails, EndpointConfiguration endpointConfiguration, List<PathVariable>? foundVariables)
    {
        var payload = endpointConfiguration.Then.Payload;
        if (!string.IsNullOrWhiteSpace(payload) && payload.IndexOf('@') != -1)
        {
            var payloadRawContent = new StringBuilder(payload);

            ReplaceGuids(payloadRawContent);
            ReplacePathVariables(requestDetails, payloadRawContent, endpointConfiguration, foundVariables);

            payload = payloadRawContent.ToString();
        }
        return payload;
    }

    private static void ReplacePathVariables(RequestDetails requestDetails, StringBuilder payload, EndpointConfiguration endpointConfiguration, List<PathVariable>? foundVariables)
    {
        if (foundVariables == default)
            return;

        foreach (var variable in foundVariables)
        {
            var varName = endpointConfiguration.When.Path.AsSpan()[variable.Name.Range].ToString();
            var varValue = requestDetails.Path.AsSpan()[variable.Value.Range].ToString();

            payload = payload.Replace(varName, varValue);
        }
    }

    private static void ReplaceGuids(StringBuilder payload)
    {
        const string guidVarName = "@guid";
        payload.Replace(guidVarName, Guid.NewGuid().ToString());
    }
}