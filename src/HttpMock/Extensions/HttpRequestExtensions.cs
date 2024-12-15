namespace HttpMock.Extensions;

public static class HttpRequestExtensions
{
    public static HttpMethodType GetHttpMethodType(this HttpRequest httpRequest)
    {
        return HttpMethodTypeParser.Parse(httpRequest.Method.AsSpan());
    }

    public static string GetNormalizedContentType(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest);

        var contentType = httpRequest.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Defaults.ContentTypes.DefaultRequestContentType;
        }

        var contentTypeSpan = httpRequest.ContentType.AsSpan();
        var semicolonPos = contentTypeSpan.IndexOf(';');
        if (semicolonPos != -1)
            return contentTypeSpan[..semicolonPos].ToString();

        return contentType;
    }
}
