using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestCondition
    {
        public RequestCondition(string? url, bool caseInsensitive)
        {
            Url = url;

            (var urlRegexExpression, var urlVariables) = NormalizeUrl(url);
            UrlRegexExpression = urlRegexExpression;
            UrlVariables = urlVariables;

            CaseInsensitive = caseInsensitive;
        }

        public string? Url { get; }

        public bool CaseInsensitive { get; }

        public string? UrlRegexExpression { get; }

        public string[] UrlVariables { get; }

        private static (string? UrlExpression, string[] UrlVariables) NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (url, Array.Empty<string>());

            (var urlExpression, var urlVariables) = NormalizeSearchExpression(url);

            if (urlExpression.EndsWith("/"))
                urlExpression = urlExpression.TrimEnd('/');

            return (urlExpression, urlVariables);
        }

        private static (string UrlExpression, string[] UrlVariables) NormalizeSearchExpression(string url)
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

            var matchedVariables = Regex.Matches(url, @"(?<name>@[\w]{1,}([\w\-\._]){0,})", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match matchedVariable in matchedVariables)
            {
                fields.Add(matchedVariable.Value.TrimStart('@'));

                url = url.Replace(matchedVariable.Value, $"(?<{matchedVariable.Value.TrimStart('@')}>[\\w]{{1,}}([\\w\\-\\._]){{0,}})");
            }

            return (url, fields.ToArray());
        }
    }
}
