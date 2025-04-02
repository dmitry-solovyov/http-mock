using HttpMock.Configuration;
using HttpMock.Models;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public class MockedRequestEndpointConfigurationResolver : IMockedRequestEndpointConfigurationResolver
{
    private readonly IConfigurationStorage _configurationStorage;
    private enum SegmentsComparisonOutcome { NoEqual = 0, Equal, EqualAsVariable, EqualAny }

    public MockedRequestEndpointConfigurationResolver(IConfigurationStorage configurationStorage)
    {
        _configurationStorage = configurationStorage;
    }

    public bool TryGetEndpointConfiguration(
        ref readonly RequestDetails requestDetails,
        out EndpointConfiguration? foundEndpointConfiguration, out List<PathVariable>? foundVariables)
    {
        if (!_configurationStorage.TryGetConfiguration(out Models.Configuration configuration))
        {
            foundEndpointConfiguration = default;
            foundVariables = default;
            return false;
        }

        EndpointConfiguration? configurationCandidate = default;
        List<PathVariable>? variablesForCandidate = default;

        foreach (var endpointConfiguration in configuration!.Endpoints)
        {
            if (requestDetails.HttpMethod != endpointConfiguration.When.HttpMethod)
                continue;

            var (samePath, pathVariables) = HasSamePath(in requestDetails, endpointConfiguration);
            if (!samePath)
                continue;

            var (sameQueryParameters, queryVariables) = HasSameQueryParameters(in requestDetails, endpointConfiguration);
            if (!sameQueryParameters)
                continue;

            if (configurationCandidate == default || endpointConfiguration.CallCounter < configurationCandidate.CallCounter)
            {
                configurationCandidate = endpointConfiguration;
                variablesForCandidate = MergeVariables(pathVariables, queryVariables);
            }
        }

        configurationCandidate?.IncreaseCounter();

        foundEndpointConfiguration = configurationCandidate;
        foundVariables = variablesForCandidate;
        return configurationCandidate != default;
    }

    public static (bool Result, List<PathVariable>? Variables) HasSamePath(ref readonly RequestDetails requestDetails, EndpointConfiguration endpointConfiguration)
    {
        var requestPathSpan = requestDetails.Path.AsSpan();
        var requestPath = requestDetails.PathParts.PathWithoutQuery;

        var configPathSpan = endpointConfiguration.When.Path.AsSpan();
        var configPath = endpointConfiguration.When.PathParts.PathWithoutQuery;

        if (requestPath.Subdirectories.Length != configPath.Subdirectories.Length)
            return (false, default);

        List<PathVariable>? variables = null;

        for (int subdirectoryIndex = 0; subdirectoryIndex < requestPath.Subdirectories.Length; subdirectoryIndex++)
        {
            var requestPathSubdirectory = requestPath.Subdirectories[subdirectoryIndex];
            var requestSegmentValue = requestPathSpan[requestPathSubdirectory.Segment.Range];

            var configPathSubdirectory = configPath.Subdirectories[subdirectoryIndex];
            var configSegmentValue = configPathSpan[configPathSubdirectory.Segment.Range];

            var comparisonResult = CompareSegments(in requestSegmentValue, in configSegmentValue);
            if (comparisonResult == SegmentsComparisonOutcome.NoEqual)
                return (false, default);

            if (comparisonResult == SegmentsComparisonOutcome.EqualAsVariable)
            {
                variables ??= [];
                variables.Add(new(new StringSegment(configPathSubdirectory.Segment.Start, configPathSubdirectory.Segment.End), requestPathSubdirectory.Segment));
            }
        }

        return (true, variables);
    }

    public record struct PathVariable(StringSegment Name, StringSegment Value);

    public static (bool Result, List<PathVariable>? Variables) HasSameQueryParameters(ref readonly RequestDetails requestDetails, EndpointConfiguration endpointConfiguration)
    {
        var requestPath = requestDetails.Path;
        var requestPathSpan = requestPath.AsSpan();

        var configQueryParameters = endpointConfiguration.When.PathParts.Query.Parameters;
        if (configQueryParameters == null || configQueryParameters.Length == 0)
            return (true, default);

        List<PathVariable>? variables = null;

        foreach (var configQueryParameter in configQueryParameters)
        {
            var configPathSpan = endpointConfiguration.When.Path.AsSpan();
            var configNameSpan = configPathSpan[configQueryParameter.NameSegment.Range];

            var requestQueryParameters = requestDetails.PathParts.Query.Parameters;

            if (configQueryParameter.IsVariable && (requestQueryParameters?.Length).GetValueOrDefault() == 0)
                continue;

            var requestQueryParameter = FindParameterWithName(in requestPathSpan, requestQueryParameters, in configNameSpan);

            var (isEqual, foundVariable) = HasSameParameterValue(in requestPathSpan, in configPathSpan, requestQueryParameter, configQueryParameter);
            if (isEqual)
            {
                if (foundVariable != null)
                {
                    variables ??= [];
                    variables.Add(new(configQueryParameter.ValueSegment, foundVariable.Value.Value));
                }

                continue;
            }
            return (false, default);
        }

        return (true, variables);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "<Pending>")]
    private static QueryParameterPart? FindParameterWithName(ref readonly ReadOnlySpan<char> path, QueryParameterPart[]? parameters, ref readonly ReadOnlySpan<char> parameterName)
    {
        if (parameters != default)
            foreach (var requestQueryParameter in parameters)
            {
                if (path[requestQueryParameter.NameSegment.Range].SequenceEqual(parameterName, CharComparer.OrdinalIgnoreCase))
                    return requestQueryParameter;
            }

        return default;
    }

    public static (bool IsEqual, PathVariable? Variable) HasSameParameterValue(
        ref readonly ReadOnlySpan<char> requestPath,
        ref readonly ReadOnlySpan<char> configPath,
        QueryParameterPart? requestQueryParameter,
        QueryParameterPart configQueryParameter)
    {
        if (configQueryParameter.ValueSegment.IsEmpty)
        {
            if ((requestQueryParameter == null || requestQueryParameter.Value.ValueSegment.IsEmpty))
            {
                return (true, default);
            }

            if (!requestQueryParameter.Value.ValueSegment.IsEmpty)
            {
                return (false, default);
            }
        }

        var parameterValueSegment = requestQueryParameter?.ValueSegment ?? default;
        if (configQueryParameter.IsVariable)
            return (true, new(configQueryParameter.NameSegment, parameterValueSegment));

        var configValueSpan = configPath[configQueryParameter.ValueSegment.Range];
        var requestParamValueSpan = requestPath[parameterValueSegment.Range];

        var comparisonResult = CompareSegments(in requestParamValueSpan, in configValueSpan);
        if (comparisonResult == SegmentsComparisonOutcome.NoEqual)
            return (false, default);

        if (comparisonResult == SegmentsComparisonOutcome.EqualAsVariable)
            return (true, new(configQueryParameter.NameSegment, parameterValueSegment));

        return (true, default);
    }

    private static SegmentsComparisonOutcome CompareSegments(
        ref readonly ReadOnlySpan<char> requestSegmentValue, ref readonly ReadOnlySpan<char> configSegmentValue)
    {
        if ((configSegmentValue.Length >= 1 && configSegmentValue[0] == '@') ||
            (configSegmentValue.Length >= 2 && configSegmentValue[0] == '/' && configSegmentValue[1] == '@'))
            return SegmentsComparisonOutcome.EqualAsVariable;

        if (requestSegmentValue.Equals(configSegmentValue, StringComparison.OrdinalIgnoreCase))
            return SegmentsComparisonOutcome.Equal;

        return SegmentsComparisonOutcome.NoEqual;
    }

    private static List<PathVariable>? MergeVariables(List<PathVariable>? variables1, List<PathVariable>? variables2)
    {
        if (variables1 == default && variables2 == default)
            return default;

        if (variables1 == default)
            return variables2;

        if (variables2 == default)
            return variables1;

        return variables1!.Concat(variables2!).ToList();
    }
}