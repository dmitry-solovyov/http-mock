using HttpMock.Helpers;

namespace HttpMock.Models;

public record struct RequestDetails(HttpMethodType HttpMethod, string Path)
{
    private bool _pathPartInitialized = false;
    private PathParts _pathParts = default;
    public PathParts PathParts
    {
        get
        {
            if (!_pathPartInitialized)
            {
                var path = Path.AsSpan();
                _pathParts = PathStringHelper.GetPathParts(in path);
                _pathPartInitialized = true;
            }
            return _pathParts;
        }
    }
}
