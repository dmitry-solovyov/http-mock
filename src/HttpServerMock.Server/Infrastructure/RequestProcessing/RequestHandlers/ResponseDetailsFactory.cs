namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers;

public static class ResponseDetailsFactory
{
    public static HttpResponseDetails Status400BadRequest()
        => new HttpResponseDetails { StatusCode = StatusCodes.Status400BadRequest };

    public static HttpResponseDetails Status404NotFound()
        => new HttpResponseDetails { StatusCode = StatusCodes.Status404NotFound };

    public static HttpResponseDetails Status204NoContent()
        => new HttpResponseDetails { StatusCode = StatusCodes.Status204NoContent };

    public static HttpResponseDetails Status202Accepted()
        => new HttpResponseDetails { StatusCode = StatusCodes.Status202Accepted };

    public static HttpResponseDetails Status200OK(string? content = null)
        => new HttpResponseDetails { StatusCode = StatusCodes.Status200OK, Content = content };
}
