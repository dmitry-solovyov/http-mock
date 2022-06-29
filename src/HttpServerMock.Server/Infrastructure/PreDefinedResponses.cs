namespace HttpServerMock.Server.Infrastructure;

public static class PreDefinedResponses
{
    public static readonly Lazy<Models.ResponseDetails> Status400BadRequest =
        new Lazy<Models.ResponseDetails>(() => new Models.ResponseDetails { StatusCode = StatusCodes.Status400BadRequest });

    public static readonly Lazy<Models.ResponseDetails> Status404NotFound =
        new Lazy<Models.ResponseDetails>(() => new Models.ResponseDetails { StatusCode = StatusCodes.Status404NotFound });

    public static readonly Lazy<Models.ResponseDetails> Status204NoContent =
        new Lazy<Models.ResponseDetails>(() => new Models.ResponseDetails { StatusCode = StatusCodes.Status204NoContent });

    public static readonly Lazy<Models.ResponseDetails> Status202Accepted =
        new Lazy<Models.ResponseDetails>(() => new Models.ResponseDetails { StatusCode = StatusCodes.Status202Accepted });

    public static readonly Lazy<Models.ResponseDetails> Status200OK =
        new Lazy<Models.ResponseDetails>(() => new Models.ResponseDetails { StatusCode = StatusCodes.Status200OK });
}
