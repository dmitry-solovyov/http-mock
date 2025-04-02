using HttpMock.Helpers;

namespace HttpMock.Models;

public sealed record class Configuration(EndpointConfiguration[] Endpoints);

public sealed record class EndpointConfiguration(EndpointRequestConfiguration When, EndpointResponseConfiguration Then)
{
    public uint CallCounter { get; private set; } = default;
    public void ResetCounter() { CallCounter = default; }
    public void IncreaseCounter() { CallCounter = CallCounter < uint.MaxValue ? CallCounter + 1 : 1; }
}

public sealed record class EndpointRequestConfiguration(HttpMethodType HttpMethod, string Path)
{
    private bool _pathPartsInitialized = false;
    private PathParts _pathParts = default;
    public PathParts PathParts
    {
        get
        {
            if (!_pathPartsInitialized)
            {
                var path = Path.AsSpan();
                _pathParts = PathStringHelper.GetPathParts(in path);
                _pathPartsInitialized = true;
            }
            return _pathParts;
        }
    }
}

public sealed record class EndpointResponseConfiguration(
    ushort StatusCode,
    string ContentType,
    string? Payload = default,
    ushort Delay = default,
    IReadOnlyDictionary<string, string?>? Headers = default);

