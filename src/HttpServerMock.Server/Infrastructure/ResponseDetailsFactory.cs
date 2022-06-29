using HttpServerMock.Server.Models;

namespace HttpServerMock.Server.Infrastructure;

public static class ResponseDetailsFactory
{
    public static ResponseDetails Status400BadRequest()
        => new ResponseDetails { StatusCode = StatusCodes.Status400BadRequest };

    public static ResponseDetails Status404NotFound()
        => new ResponseDetails { StatusCode = StatusCodes.Status404NotFound };

    public static ResponseDetails Status204NoContent()
        => new ResponseDetails { StatusCode = StatusCodes.Status204NoContent };

    public static ResponseDetails Status202Accepted()
        => new ResponseDetails { StatusCode = StatusCodes.Status202Accepted };

    public static ResponseDetails Status200OK(string? content = null)
        => new ResponseDetails { StatusCode = StatusCodes.Status200OK, Content = content };
}
