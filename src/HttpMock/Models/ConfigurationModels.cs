using HttpMock.Extensions;
using HttpMock.Helpers;

namespace HttpMock.Models;

public sealed record class DomainConfiguration(string Domain, EndpointConfiguration[] Endpoints);

public sealed record class EndpointConfiguration(EndpointRequestConfiguration When, EndpointResponseConfiguration Then)
{
    public uint CallCounter { get; private set; } = default;
    public void ResetCounter() { CallCounter = default; }
    public void IncreaseCounter() { CallCounter = CallCounter < uint.MaxValue ? CallCounter + 1 : 1; }
}

public sealed record class EndpointRequestConfiguration(HttpMethodType HttpMethod, string Path)
{
    private bool _pathSegmentsInitialized = false;
    private StringSegment[]? _pathSegments = default;
    public Span<StringSegment> PathSegments
    {
        get
        {
            if (!_pathSegmentsInitialized)
            {
                var path = Path.AsSpan();
                path = path[PathStringHelper.GetPathWithoutQuery(in path)];
                _pathSegments = path.SplitBy('/');
                _pathSegmentsInitialized = true;
            }
            return _pathSegments;
        }
    }

    private bool _pathPartInitialized = false;
    private StringSegment _pathPart = default;
    public StringSegment PathPart
    {
        get
        {
            if (!_pathPartInitialized)
            {
                var path = Path.AsSpan();
                _pathPart = PathStringHelper.GetPathParts(in path, false).Path;
                _pathPartInitialized = true;
            }
            return _pathPart;
        }
    }

    private bool _queryParametersInitialized = false;
    private QueryParameterRef[]? _queryParameters = default;
    public QueryParameterRef[] QueryParameters
    {
        get
        {
            if (!_queryParametersInitialized)
            {
                var path = Path.AsSpan();
                var queryParameters = PathStringHelper.GetQueryParametersRef(in path);
                _queryParameters = queryParameters ?? [];
                _queryParametersInitialized = true;
            }
            return _queryParameters!;
        }
    }
}

public sealed record class QueryParameter(string Name, string Value);

public sealed record class EndpointResponseConfiguration(
    ushort StatusCode,
    string ContentType,
    string? Payload = default,
    ushort Delay = default,
    IReadOnlyDictionary<string, string?>? Headers = default);

