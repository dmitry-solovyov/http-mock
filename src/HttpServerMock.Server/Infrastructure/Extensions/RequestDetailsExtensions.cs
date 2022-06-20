using HttpServerMock.RequestDefinitions;
using System.Collections.Generic;

namespace HttpServerMock.Server.Infrastructure.Extensions
{
    public static class RequestDetailsExtensions
    {
        public static bool IsCommandRequest(this IRequestDetails? requestDetails, out string? commandName)
        {
            commandName = null;

            if (requestDetails == null)
                return false;

            var commandHeader = GetHeaderValue(requestDetails.Headers, Constants.HeaderNames.ManagementCommandRequestHeader);
            if (string.IsNullOrWhiteSpace(commandHeader))
                return false;

            commandName = commandHeader;
            return true;
        }

        private static string? GetHeaderValue(IReadOnlyDictionary<string, string>? headers, string headerName)
        {
            if (headers != null && headers.TryGetValue(headerName, out var foundHeader))
                return foundHeader;

            return null;
        }
    }
}