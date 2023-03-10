using System.Text.RegularExpressions;

namespace HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;

public class ConfigurationStorageItemEndpointFilter
{
    public ConfigurationStorageItemEndpointFilter(string? url, bool caseInsensitive = true)
    {
        Url = url;

        (UrlRegexExpression, UrlVariables) = NormalizeUrl(url);

        CaseInsensitive = caseInsensitive;
    }

    public string? Url { get; }

    public bool CaseInsensitive { get; }

    public string? UrlRegexExpression { get; }

    public IReadOnlyCollection<string> UrlVariables { get; }

    private static (string? UrlExpression, IReadOnlyCollection<string> UrlVariables) NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (url, Array.Empty<string>());

        (var urlExpression, var urlVariables) = NormalizeSearchExpression(url);

        if (urlExpression.EndsWith("/"))
            urlExpression = urlExpression.TrimEnd('/');

        return (urlExpression, urlVariables);
    }

    private static (string UrlExpression, IReadOnlyCollection<string> UrlVariables) NormalizeSearchExpression(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (url, Array.Empty<string>());

        var fields = new List<string>();

        if (url.Contains("?"))
            url = url.Replace("?", "\\?");

        if (url.Contains("*"))
            url = url.Replace("*", ".?");

        if (!url.Contains("@"))
            return (url, fields.ToArray());

        var matchedVariables = Regex.Matches(url, @"(?<name>@[\w]{1,}([\w\-\._]){0,})", RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromMilliseconds(10));

        foreach (Match matchedVariable in matchedVariables)
        {
            fields.Add(matchedVariable.Value.TrimStart('@'));

            url = url.Replace(matchedVariable.Value, $"(?<{matchedVariable.Value.TrimStart('@')}>[\\w]{{1,}}([\\w\\-\\._]){{0,}})");
        }

        return (url, fields.ToArray());
    }
}
