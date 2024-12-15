using HttpMock.Models;
using HttpMock.RequestProcessing;
using System.Text;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestHandler : IMockedRequestHandler
{
    private const int MaxDelay = 60_000;

    private readonly IMockedRequestEndpointConfigurationResolver _mockedRequestEndpointConfigurationResolver;

    public MockedRequestHandler(IMockedRequestEndpointConfigurationResolver mockedRequestEndpointConfigurationResolver)
    {
        _mockedRequestEndpointConfigurationResolver = mockedRequestEndpointConfigurationResolver;
    }

    public async ValueTask Execute(MockedRequestDetails requestDetails, HttpResponse httpResponse, CancellationToken cancellationToken = default)
    {
        if (_mockedRequestEndpointConfigurationResolver.TryGetEndpointConfiguration(ref requestDetails, out var endpointConfiguration))
        {
            await ApplyEndpointConfiguration(requestDetails, httpResponse, endpointConfiguration!, cancellationToken).ConfigureAwait(false);
            return;
        }

        httpResponse.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnknownMockedResponse);
    }

    private static ValueTask ApplyEndpointConfiguration(MockedRequestDetails requestDetails, HttpResponse httpResponse, EndpointConfiguration endpointConfiguration, CancellationToken cancellationToken = default)
    {
        return ProcessMockedRequest(requestDetails.Path, endpointConfiguration, httpResponse, cancellationToken);
    }

    private static async ValueTask ProcessMockedRequest(string queryPath, EndpointConfiguration endpointConfiguration, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        var delay = GetDelay(endpointConfiguration);
        if (delay > 0)
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        var statusCode = GetStatusCode(endpointConfiguration);
        var contentType = GetContentType(endpointConfiguration);
        var content = GetPayload(queryPath, endpointConfiguration);

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

    private static string? GetPayload(string queryPath, EndpointConfiguration endpointConfiguration)
    {
        var payload = endpointConfiguration.Then.Payload;
        if (!string.IsNullOrWhiteSpace(payload))
        {
            //var payloadRawContent = new StringBuilder(payload);

            //paylopayloadRawContentad = ReplaceGuids(payloadRawContent);

            //if (endpointConfiguration.When.PathRegex != default && endpointConfiguration.When.PathVariables != default)
            //    foreach (var urlVariable in endpointConfiguration.When.PathVariables)
            //    {
            //        var regexVarName = urlVariable;
            //        while (payload.Contains(regexVarName))
            //        {
            //            var match = Regex.Match(queryPath, endpointConfiguration.When.PathRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            //            payload = payload.Replace(regexVarName, match.Groups[urlVariable[1..]]?.Value);
            //        }
            //    }
        }
        return payload;
    }

    private static void ReplaceGuids(StringBuilder payload)
    {
        const string guidVarName = "@guid";

        var comparison = StringComparison.OrdinalIgnoreCase;

        //while (payload.Sp.IndexOf(guidVarName, comparison) != -1)
        //    payload = payload.Replace(guidVarName, Guid.NewGuid().ToString(), comparison);

        //return payload;
    }
}