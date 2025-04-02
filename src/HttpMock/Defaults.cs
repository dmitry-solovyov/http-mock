using System.Net.Mime;

namespace HttpMock;

public static class Defaults
{
    public static class StatusCodes
    {
        public const ushort StatusCodeForProcessedReadCommands = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;
        public const ushort StatusCodeForProcessedUpdateCommands = Microsoft.AspNetCore.Http.StatusCodes.Status202Accepted;
        public const ushort StatusCodeForUnhandledRequests = Microsoft.AspNetCore.Http.StatusCodes.Status501NotImplemented;
        public const ushort StatusCodeForUnknownCommand = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest;
        public const ushort StatusCodeForUnknownCommandMethodType = Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed;
        public const ushort StatusCodeForUnknownMockedResponse = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound;

    }

    public static class ContentTypes
    {
        public const string ContentTypeForUntypedResponse = MediaTypeNames.Text.Plain;
        public const string ContentTypeForMockedResponse = MediaTypeNames.Application.Json;
        public const string DefaultRequestContentType = MediaTypeNames.Application.Json;
    }
}
