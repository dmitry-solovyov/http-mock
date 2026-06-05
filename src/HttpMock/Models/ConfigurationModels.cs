using HttpMock.Helpers;

namespace HttpMock.Models;

public sealed record class Configuration(EndpointConfiguration[] Endpoints);

public sealed record class EndpointConfiguration(EndpointRequestConfiguration When, EndpointResponseConfiguration Then)
{
    private uint _callCounter = 0;
    public uint CallCounter => _callCounter;
    public void ResetCounter() => Interlocked.Exchange(ref _callCounter, 0);
    public void IncreaseCounter() { Interlocked.Increment(ref _callCounter); }
}

public sealed record class EndpointRequestConfiguration(HttpMethodType HttpMethod, string Path)
{
    private readonly Lazy<PathParts> _pathParts = new Lazy<PathParts>(() =>
    {
        var p = Path.AsSpan();
        return PathStringHelper.GetPathParts(in p);
    });
    public PathParts PathParts => _pathParts.Value;
}

public sealed record class EndpointResponseConfiguration(
    ushort StatusCode,
    string ContentType,
    string? Payload = default,
    ushort Delay = default,
    IReadOnlyDictionary<string, string?>? Headers = default)
{
    public bool PayloadContainVariables => !string.IsNullOrWhiteSpace(Payload) && Payload.IndexOf('@') != -1;
}

