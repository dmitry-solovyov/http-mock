namespace HttpMock;

public enum HttpMethodType
{
    None,
    Get,
    Post,
    Put,
    Delete,
    Options,
    Head,
    Patch,
    Trace,
    Connect
}

public static class HttpMethodTypeExtensions
{
    public static string GetMethodName(this HttpMethodType httpMethodType)
    {
        return httpMethodType switch
        {
            HttpMethodType.Get => HttpMethods.Get,
            HttpMethodType.Post => HttpMethods.Post,
            HttpMethodType.Put => HttpMethods.Put,
            HttpMethodType.Delete => HttpMethods.Delete,
            HttpMethodType.Options => HttpMethods.Options,
            HttpMethodType.Head => HttpMethods.Head,
            HttpMethodType.Patch => HttpMethods.Patch,
            HttpMethodType.Trace => HttpMethods.Trace,
            HttpMethodType.Connect => HttpMethods.Connect,
            _ => string.Empty
        };
    }
}

public static class HttpMethodTypeParser
{
    public static HttpMethodType Parse(ReadOnlySpan<char> httpMethod, HttpMethodType defaultValue = HttpMethodType.None) =>
        httpMethod switch
        {
            var s when SameCommand(s, HttpMethods.Get) => HttpMethodType.Get,
            var s when SameCommand(s, HttpMethods.Post) => HttpMethodType.Post,
            var s when SameCommand(s, HttpMethods.Put) => HttpMethodType.Put,
            var s when SameCommand(s, HttpMethods.Delete) => HttpMethodType.Delete,
            var s when SameCommand(s, HttpMethods.Patch) => HttpMethodType.Patch,
            var s when SameCommand(s, HttpMethods.Options) => HttpMethodType.Options,
            var s when SameCommand(s, HttpMethods.Head) => HttpMethodType.Head,
            var s when SameCommand(s, HttpMethods.Trace) => HttpMethodType.Trace,
            var s when SameCommand(s, HttpMethods.Connect) => HttpMethodType.Connect,

            _ => defaultValue
        };

    private static bool SameCommand(ReadOnlySpan<char> methodA, ReadOnlySpan<char> methodB)
    {
        return MemoryExtensions.Equals(methodA, methodB, StringComparison.OrdinalIgnoreCase);
    }
}
