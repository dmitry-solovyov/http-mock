using Microsoft.AspNetCore.Http.Extensions;

namespace HttpMock.RequestProcessing;

public readonly record struct RequestDetails(string? CommandName, string Domain, HttpMethodType HttpMethod, string QueryPath, string ContentType, Stream RequestBody);

public interface IRequestDetailsProvider
{
    bool TryGetRequestDetails(HttpContext httpContext, out RequestDetails requestDetails);
}

public class RequestDetailsProvider : IRequestDetailsProvider
{
    private const string CommandNameHeader = "X-HttpMock-Command";
    private const string CommandDomainHeader = "X-HttpMock-Domain";

    public bool TryGetRequestDetails(HttpContext httpContext, out RequestDetails requestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            requestDetails = default;
            return false;
        }

        var headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        TryGetCommandNameFromHeader(headers, out var commandName);

        TryGetDomainAndQueryPathFromUrl(request.GetEncodedPathAndQuery(), out var domainFromUrl, out var queryPath);

        TryGetDomainFromHeader(headers, out var domainFromHeader);

        var domain = domainFromHeader ?? domainFromUrl ?? string.Empty;

        var httpMethodType = HttpMethodTypeParser.Parse(request.Method);

        var contentType = request.ContentType ?? Defaults.ContentTypes.DefaultRequestContentType;

        requestDetails = new RequestDetails(commandName, domain, httpMethodType, queryPath, contentType, request.Body);
        return true;
    }

    private static bool TryGetDomainAndQueryPathFromUrl(string inputUrl, out string? domain, out string queryPath)
    {
        if (!string.IsNullOrWhiteSpace(inputUrl))
        {
            var inputUrlSpan = inputUrl.AsSpan();

            if (inputUrlSpan[0] == '/')
                inputUrlSpan = inputUrlSpan.TrimStart('/');

            var index = inputUrlSpan.IndexOf('/');
            if (index > 0)
            {
                domain = inputUrlSpan.Slice(0, index).ToString();
                queryPath = inputUrlSpan.Slice(index).ToString();
                return true;
            }
        }

        domain = default;
        queryPath = inputUrl;
        return false;
    }

    private static bool TryGetCommandNameFromHeader(Dictionary<string, string> headers, out string? commandName) =>
        IsValuePresentInHeader(headers, CommandNameHeader, out commandName);

    private static bool TryGetDomainFromHeader(Dictionary<string, string> headers, out string? domain) =>
        IsValuePresentInHeader(headers, CommandDomainHeader, out domain);

    private static bool IsValuePresentInHeader(Dictionary<string, string> headers, string headerKey, out string? headerValue) =>
        headers.TryGetValue(headerKey, out headerValue) && !string.IsNullOrEmpty(headerValue);
}