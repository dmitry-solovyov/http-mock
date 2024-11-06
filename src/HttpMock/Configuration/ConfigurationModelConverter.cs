using HttpMock.Models;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace HttpMock.Configuration;

internal static partial class RegexHelper
{
    [GeneratedRegex(@"(?<name>@[\w]{1,}([\w\-\._]){0,})", RegexOptions.IgnoreCase | RegexOptions.Singleline, 50)]
    internal static partial Regex VariablesNameRegex();
}

internal static class ConfigurationModelConverter
{
    internal static class ModelToDto
    {
        public static DomainConfigurationDto? Convert(DomainConfiguration? domainConfiguration)
        {
            if (domainConfiguration == default)
                return default;

            var endpoints = domainConfiguration.Endpoints.Select(Convert).ToArray() ?? Array.Empty<EndpointConfigurationDto>();
            return new DomainConfigurationDto { Endpoints = endpoints };
        }

        private static EndpointConfigurationDto Convert(EndpointConfiguration endpointConfiguration)
        {
            var delay = endpointConfiguration.Then.Delay == default ? (int?)null : endpointConfiguration.Then.Delay;

            return new EndpointConfigurationDto
            {
                Description = endpointConfiguration.Description,
                Method = endpointConfiguration.When.HttpMethod.ToString(),
                Url = endpointConfiguration.When.Url,
                ContentType = endpointConfiguration.Then.ContentType,
                Status = endpointConfiguration.Then.StatusCode,
                Delay = delay,
                Payload = endpointConfiguration.Then.Payload,
                Headers = endpointConfiguration.Then.Headers?.ToDictionary(x => x.Key, x => x.Value),
                ProxyUrl = endpointConfiguration.Then.ProxyUrl,
                CallbackUrl = endpointConfiguration.Then.CallbackUrl
            };
        }
    }

    internal static class DtoToModel
    {
        private const int DefaultStatusCode = StatusCodes.Status200OK;
        private const string DefaultHttpMethod = "GET";
        private const string DefaultContentType = MediaTypeNames.Application.Json;

        public static DomainConfiguration? Convert(string domain, DomainConfigurationDto? domainConfigurationDto)
        {
            ArgumentException.ThrowIfNullOrEmpty(domain);

            if (domainConfigurationDto == default)
                return default;

            var endpoints = domainConfigurationDto.Endpoints!.Select(Convert).ToArray() ?? Array.Empty<EndpointConfiguration>();
            return new DomainConfiguration(domain, endpoints);
        }

        private static EndpointConfiguration Convert(EndpointConfigurationDto endpointConfigurationDto)
        {
            ArgumentException.ThrowIfNullOrEmpty(endpointConfigurationDto.Url);

            var statusCode = endpointConfigurationDto.Status ?? DefaultStatusCode;

            var httpMethod = endpointConfigurationDto.Method ?? DefaultHttpMethod;
            var httpMethodType = HttpMethodTypeParser.Parse(httpMethod);

            var contentType = endpointConfigurationDto.ContentType ?? DefaultContentType;
            var delay = endpointConfigurationDto.Delay.GetValueOrDefault();

            var url = endpointConfigurationDto.Url;
            var (urlRegexExpression, urlVariables) = NormalizeUrl(url);

            var when = new EndpointRequestConfiguration(httpMethodType, endpointConfigurationDto.Url, urlRegexExpression, urlVariables);

            Uri.TryCreate(endpointConfigurationDto.ProxyUrl, UriKind.Absolute, out var proxyUri);
            Uri.TryCreate(endpointConfigurationDto.CallbackUrl, UriKind.Absolute, out var callbackUri);

            var then = new EndpointResponseConfiguration(
                statusCode,
                contentType,
                endpointConfigurationDto.Payload,
                delay,
                endpointConfigurationDto.Headers,
                proxyUri?.ToString(),
                callbackUri?.ToString()
            );

            return new EndpointConfiguration(when, then, endpointConfigurationDto.Description);
        }

        private static (string? UrlRegexExpression, IReadOnlyCollection<string>? UrlVariables) NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (url, default);

            var (urlRegexExpression, urlVariables) = NormalizeUrlRegexExpression(url);

            urlRegexExpression?.TrimEnd('/');

            return (urlRegexExpression, urlVariables);
        }

        private static (string UrlRegexExpression, IReadOnlyCollection<string>? UrlVariables) NormalizeUrlRegexExpression(string url)
        {
            if (url.Contains('?'))
                url = url.Replace("?", "\\?");

            if (url.Contains('*'))
                url = url.Replace("*", ".?");

            if (!url.Contains('@'))
                return (url, default);

            var matchedVariables = RegexHelper.VariablesNameRegex().Matches(url);

            var fields = new List<string>();

            foreach (Match matchedVariable in matchedVariables)
            {
                var fieldName = matchedVariable.Value.TrimStart('@');
                fields.Add(fieldName);

                url = url.Replace(matchedVariable.Value, $"(?<{fieldName}>[\\w]{{1,}}([\\w\\-\\._\\-%\\+\\*\\,\\;]){{0,}})");
            }

            return (url, fields);
        }
    }
}
