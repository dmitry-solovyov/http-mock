using HttpMock.Configuration;
using HttpMock.Extensions;
using HttpMock.Models;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestEndpointConfigurationResolver : IMockedRequestEndpointConfigurationResolver
{
    private readonly IConfigurationStorage _configurationStorage;

    public MockedRequestEndpointConfigurationResolver(IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
    }

    public bool TryGetEndpointConfiguration(ref readonly MockedRequestDetails requestDetails, out EndpointConfiguration? foundEndpointConfiguration)
    {
        if (!_configurationStorage.TryGetDomainConfiguration(requestDetails.Domain, out DomainConfiguration? domainConfiguration))
        {
            foundEndpointConfiguration = default;
            return false;
        }

        EndpointConfiguration? configurationCandidate = default;

        foreach (var endpointConfiguration in domainConfiguration!.Endpoints)
        {
            if (requestDetails.HttpMethod != endpointConfiguration.When.HttpMethod)
                continue;

            var samePath = HasSamePath(in requestDetails, endpointConfiguration);
            if (!samePath)
                continue;

            var sameQueryParameters = HasSameQueryParameters(in requestDetails, endpointConfiguration);
            if (!sameQueryParameters)
                continue;

            if (configurationCandidate == default)
            {
                configurationCandidate = endpointConfiguration;
            }
            else if (endpointConfiguration.CallCounter < configurationCandidate.CallCounter)
            {
                configurationCandidate = endpointConfiguration;
            }
        }

        configurationCandidate?.IncreaseCounter();

        foundEndpointConfiguration = configurationCandidate;
        return configurationCandidate != default;
    }

    public static bool HasSamePath(ref readonly MockedRequestDetails requestDetails, EndpointConfiguration endpointConfiguration)
    {
        var path = requestDetails.GetPathWithoutParameters();
        var requestPathSegments = path.SplitBy('/');
        var configPathSegments = endpointConfiguration.When.PathSegments;
        var configPath = endpointConfiguration.When.Path.AsSpan();

        if (requestPathSegments.Length != configPathSegments.Length)
            return false;

        var variables = new List<PathVariable>();

        for (int segmentIndex = 0; segmentIndex < requestPathSegments.Length; segmentIndex++)
        {
            var requestPathSegment = requestPathSegments[segmentIndex];
            var requestSegmentValue = path[requestPathSegment.Range];

            var configPathSegment = configPathSegments[segmentIndex];
            var configSegmentValue = configPath[configPathSegment.Range];

            var comparisonResult = CompareSegments(in requestSegmentValue, in configSegmentValue);
            if (comparisonResult == SegmentsComparisonOutcome.NoEqual)
                return false;

            if (comparisonResult == SegmentsComparisonOutcome.EqualAsVariable)
                variables.Add(new(configPathSegment, requestPathSegment));
        }

        return true;
    }

    public record struct PathVariable(StringSegment VariableConfigName, StringSegment PathVariableValue);

    public enum SegmentsComparisonOutcome { NoEqual, Equal, EqualAsVariable, EqualAny }

    public static bool HasSameQueryParameters(ref readonly MockedRequestDetails requestDetails, EndpointConfiguration endpointConfiguration)
    {
        var requestPath = requestDetails.Path;
        var requestPathSpan = requestPath.AsSpan();

        var configQueryParameters = endpointConfiguration.When.QueryParameters;
        if (configQueryParameters == null || configQueryParameters.Length == 0)
            return true;

        var variables = new List<PathVariable>();

        foreach (var configQueryParameter in configQueryParameters)
        {
            var configPathSpan = endpointConfiguration.When.Path.AsSpan();
            var configNameSpan = configPathSpan[configQueryParameter.Name.Range];
            var configValueSpan = configPathSpan[configQueryParameter.Value.Range];

            var requestQueryParameters = requestDetails.QueryParameters;

            if (configValueSpan[0] == '@' && (requestQueryParameters == null || requestQueryParameters.Length == 0))
                continue;

            var configValueIsEmpty = configValueSpan.IsEmpty;
            var configValueIsVariable = !configValueIsEmpty && configValueSpan[0] == '@';

            var requestQueryParameter = FindParameterWithName(in requestPathSpan, requestQueryParameters, in configNameSpan);
            if (requestQueryParameter == null)
            {
                if (configValueIsEmpty)
                    continue;

                if (configValueIsVariable)
                {
                    variables.Add(new(configQueryParameter.Value, default));
                    continue;
                }
                return false;
            }

            if (configValueIsVariable)
            {
                variables.Add(new(configQueryParameter.Value, requestQueryParameter.Value.Value));
                continue;
            }

            QueryParameterRef queryParameter = requestQueryParameter.Value;
            if (!configValueSpan.SequenceEqual(requestPath[queryParameter.Value.Range], CharComparer.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static QueryParameterRef? FindParameterWithName(ref readonly ReadOnlySpan<char> path, QueryParameterRef[]? parameters, ref readonly ReadOnlySpan<char> parameterName)
    {
        if (parameters != null && parameters.Length > 0)
        {
            foreach (var requestQueryParameter in parameters)
            {
                if (path[requestQueryParameter.Name.Range].SequenceEqual(parameterName, CharComparer.OrdinalIgnoreCase))
                    return requestQueryParameter;
            }
        }
        return default;
    }

    private static SegmentsComparisonOutcome CompareSegments(
        ref readonly ReadOnlySpan<char> requestSegmentValue, ref readonly ReadOnlySpan<char> configSegmentValue)
    {
        if (configSegmentValue[0] == '@')
            return SegmentsComparisonOutcome.EqualAsVariable;

        if (requestSegmentValue.Equals(configSegmentValue, StringComparison.OrdinalIgnoreCase))
            return SegmentsComparisonOutcome.Equal;

        return SegmentsComparisonOutcome.NoEqual;
    }
}