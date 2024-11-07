using System.Net.Mime;

namespace HttpMock;

public static class Defaults
{
    public static class StatusCodes
    {
        public const int StatusCodeForUnhandledRequests = Microsoft.AspNetCore.Http.StatusCodes.Status501NotImplemented;
        public const int StatusCodeForMockedRequests = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;

    }

    public static class ContentTypes
    {
        public const string ContentTypeForUntypedResponse = MediaTypeNames.Text.Plain;
        public const string ContentTypeForMockedResponse = MediaTypeNames.Application.Json;
        public const string DefaultRequestContentType = MediaTypeNames.Application.Json;
    }
}
