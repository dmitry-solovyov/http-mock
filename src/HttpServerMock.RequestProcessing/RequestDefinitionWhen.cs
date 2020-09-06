using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionWhen
    {
        private string? _url;
        public string? Url
        {
            get => _url;
            set
            {
                _url = value;

                var (normalizedUrl, fieldNames) = NormalizeUrl(value);
                UrlRegexExpression = normalizedUrl;
                UrlVariables = fieldNames;
            }
        }

        public bool CaseInsensitive { get; set; } = true;

        public string? UrlRegexExpression { get; private set; }

        public string[] UrlVariables { get; private set; } = Array.Empty<string>();

        private static (string? NormizeledUrl, string[] FieldNames) NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (url, Array.Empty<string>());

            var (urlExpression, urlVariables) = NormalizeSearchExpression(url);

            if (urlExpression.EndsWith("/"))
                urlExpression = urlExpression.TrimEnd('/');

            return (urlExpression, urlVariables);
        }

        private static (string normizeledValue, string[] foundFieldNames) NormalizeSearchExpression(string url)
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
