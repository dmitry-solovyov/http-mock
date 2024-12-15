namespace HttpMock.Extensions;

public static class HeaderDictionaryExtensions
{
    public static string? GetValue(this IHeaderDictionary? headers, string headerKey, string? defaultValue = default)
    {
        if (headers == default)
            return defaultValue;

        if (headers.ContainsKey(headerKey) && headers[headerKey].Count > 0)
        {
            var value = headers[headerKey].FirstOrDefault();
            return value ?? defaultValue;
        }
        return defaultValue;
    }
}
