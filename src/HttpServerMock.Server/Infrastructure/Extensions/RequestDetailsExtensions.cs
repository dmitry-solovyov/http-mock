using HttpServerMock.Server.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace HttpServerMock.Server.Infrastructure.Extensions
{
    public static class RequestDetailsExtensions
    {
        public static bool IsCommandRequest(this IRequestDetails? requestDetails, out string? commandName)
        {
            commandName = null;

            if (requestDetails == null)
                return false;

            var commandHeader = GetHeaderValue(requestDetails.Headers, Constants.HeaderNames.CommandHeader);
            if (string.IsNullOrWhiteSpace(commandHeader))
                return false;

            commandName = commandHeader;
            return true;
        }

        private static string? GetHeaderValue(IReadOnlyDictionary<string, string[]>? headers, string headerName)
        {
            if (headers == null || !headers.TryGetValue(headerName, out var foundHeader))
                return null;

            var headerValue = foundHeader.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(headerValue))
                return null;

            return headerValue;
        }
    }
}
