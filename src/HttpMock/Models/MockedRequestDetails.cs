using HttpMock.Helpers;

namespace HttpMock.Models;

public record struct MockedRequestDetails(string Domain, HttpMethodType HttpMethod, string Path)
{
    public ReadOnlySpan<char> GetPathWithoutParameters()
    {
        var pathSpan = Path.AsSpan();
        return pathSpan[PathPart.Range];
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
    public QueryParameterRef[]? QueryParameters
    {
        get
        {
            if (!_queryParametersInitialized)
            {
                var path = Path.AsSpan();
                var queryParameters = PathStringHelper.GetQueryParametersRef(in path);
                _queryParameters = queryParameters;
                _queryParametersInitialized = true;
            }
            return _queryParameters!;
        }
    }

}
