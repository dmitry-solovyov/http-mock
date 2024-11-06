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

public static class HttpMethodTypeParser
{
    public static HttpMethodType Parse(string httpMethod) =>
        httpMethod switch
        {
            var s when HttpMethods.IsGet(s) => HttpMethodType.Get,
            var s when HttpMethods.IsPost(s) => HttpMethodType.Post,
            var s when HttpMethods.IsPut(s) => HttpMethodType.Put,
            var s when HttpMethods.IsDelete(s) => HttpMethodType.Delete,
            var s when HttpMethods.IsPatch(s) => HttpMethodType.Patch,
            var s when HttpMethods.IsOptions(s) => HttpMethodType.Options,
            var s when HttpMethods.IsHead(s) => HttpMethodType.Head,
            var s when HttpMethods.IsTrace(s) => HttpMethodType.Trace,
            var s when HttpMethods.IsConnect(s) => HttpMethodType.Connect,

            _ => HttpMethodType.None
        };
}


