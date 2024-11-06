using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mime;

namespace HttpMock.RequestProcessing;

public readonly record struct RequestDetails(string? CommandName, string? Domain, HttpMethodType HttpMethod, string QueryPath, string ContentType);

public interface IRequestDetailsProvider
{
    bool TryGetRouteDetails(HttpContext httpContext, out RequestDetails requestDetails);
}

public class RequestDetailsProvider : IRequestDetailsProvider
{
    private const string CommandNameHeader = "X-HttpMock-Command";
    private const string CommandDomainHeader = "X-HttpMock-Domain";

    private const string DefaultContentType = MediaTypeNames.Application.Json;

    public bool TryGetRouteDetails(HttpContext httpContext, out RequestDetails requestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            requestDetails = default;
            return false;
        }

        var headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        TryGetDomainAndQueryPathFromUrl(request.GetEncodedPathAndQuery(), out var domainFromUrl, out var queryPath);
        TryGetDomainFromHeader(headers, out var domainFromHeader);

        TryGetCommandNameFromHeader(headers, out var commandName);

        var domain = domainFromHeader ?? domainFromUrl;

        var httpMethodType = HttpMethodTypeParser.Parse(request.Method);

        requestDetails = new RequestDetails(commandName, domain, httpMethodType, queryPath, request.ContentType ?? DefaultContentType);
        return true;
    }

    private bool TryGetDomainAndQueryPathFromUrl(string inputUrl, out string? domain, out string queryPath)
    {
        if (!string.IsNullOrWhiteSpace(inputUrl))
        {
            if (inputUrl[0] == '/')
                inputUrl = inputUrl.TrimStart('/');

            var index = inputUrl.IndexOf('/');
            if (index > 0)
            {
                domain = inputUrl.AsSpan(0, index).ToString();
                queryPath = inputUrl.AsSpan(index).ToString();
                return true;
            }
        }

        domain = default;
        queryPath = inputUrl;
        return false;
    }

    public static bool TryGetCommandNameFromHeader(Dictionary<string, string> headers, out string? commandName) =>
        IsValuePresentInHeader(headers, CommandNameHeader, out commandName);

    public static bool TryGetDomainFromHeader(Dictionary<string, string> headers, out string? domain) =>
        IsValuePresentInHeader(headers, CommandDomainHeader, out domain);

    public static bool IsValuePresentInHeader(Dictionary<string, string> headers, string headerKey, out string? headerValue) =>
        headers.TryGetValue(headerKey, out headerValue) && !string.IsNullOrEmpty(headerValue);
}