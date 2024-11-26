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

        GetCommandNameFromHeader(headers, out var commandName);

        GetDomainAndQueryPathFromUrl(request.GetEncodedPathAndQuery(), out var domainFromUrl, out var queryPath);

        GetDomainFromHeader(headers, out var domainFromHeader);

        var domain = domainFromHeader ?? domainFromUrl ?? string.Empty;

        var httpMethodType = HttpMethodTypeParser.Parse(request.Method);

        var contentType = GetNormalizedContentType(request.ContentType);

        requestDetails = new RequestDetails(commandName, domain, httpMethodType, queryPath, contentType, request.Body);
        return true;
    }

    private static void GetDomainAndQueryPathFromUrl(string inputUrl, out string? domain, out string queryPath)
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
                return;
            }
        }

        domain = default;
        queryPath = inputUrl;
    }

    private static void GetCommandNameFromHeader(Dictionary<string, string> headers, out string? commandName) =>
        IsValuePresentInHeader(headers, CommandNameHeader, out commandName);

    private static void GetDomainFromHeader(Dictionary<string, string> headers, out string? domain) =>
        IsValuePresentInHeader(headers, CommandDomainHeader, out domain);

    private static bool IsValuePresentInHeader(Dictionary<string, string> headers, string headerKey, out string? headerValue) =>
        headers.TryGetValue(headerKey, out headerValue) && !string.IsNullOrEmpty(headerValue);

    private static string GetNormalizedContentType(string? contentType)
    {
        if (contentType == null)
        {
            contentType = Defaults.ContentTypes.DefaultRequestContentType;
        }
        else if (contentType.IndexOf(';') >= 0)
        {
            contentType = contentType.Substring(0, contentType.IndexOf(';'));
        }
        return contentType;
    }
}