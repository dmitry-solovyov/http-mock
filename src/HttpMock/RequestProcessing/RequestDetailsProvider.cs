using HttpMock.Extensions;
using HttpMock.Helpers;
using HttpMock.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace HttpMock.RequestProcessing;

public interface IRequestDetailsProvider
{
    bool TryGetCommandRequestDetails(HttpContext httpContext, out CommandRequestDetails commandRequestDetails);

    bool TryGetRequestDetails(HttpContext httpContext, out MockedRequestDetails requestDetails);
}

public class RequestDetailsProvider : IRequestDetailsProvider
{
    private const string CommandNameHeader = "X-HttpMock-Command";
    private const string CommandDomainHeader = "X-HttpMock-Domain";

    public bool TryGetCommandRequestDetails(HttpContext httpContext, out CommandRequestDetails commandRequestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            commandRequestDetails = default;
            return false;
        }

        var commandName = request.Headers.GetValue(CommandNameHeader);
        if (string.IsNullOrEmpty(commandName))
        {
            commandRequestDetails = default;
            return false;
        }

        var domain = request.Headers.GetValue(CommandDomainHeader) ?? string.Empty;
        var httpMethodType = request.GetHttpMethodType();
        var contentType = request.GetNormalizedContentType();

        commandRequestDetails = new CommandRequestDetails(commandName, domain, httpMethodType, contentType, request.Body);
        return true;
    }

    public bool TryGetRequestDetails(HttpContext httpContext, out MockedRequestDetails requestDetails)
    {
        var request = httpContext?.Request;
        if (request == default)
        {
            requestDetails = default;
            return false;
        }

        var requestPath = request.GetEncodedPathAndQuery();

        var requestPathSpan = requestPath.AsSpan();
        var (domainSegment, pathSegment) = PathStringHelper.SplitDomainAndPath(in requestPathSpan);

        var domain = requestPath[domainSegment.Range];
        var path = requestPath[pathSegment.Range];

        var httpMethodType = request.GetHttpMethodType();

        requestDetails = new MockedRequestDetails(domain, httpMethodType, path);
        return true;
    }
}