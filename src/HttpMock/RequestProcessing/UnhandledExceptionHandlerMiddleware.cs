namespace HttpMock.RequestProcessing;

public class UnhandledExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private ILogger<UnhandledExceptionHandlerMiddleware>? _logger;

    public UnhandledExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unhandled exception {ex.Message}{Environment.NewLine}{ex.StackTrace}!";

            _logger ??= httpContext.RequestServices.GetService<ILogger<UnhandledExceptionHandlerMiddleware>>();
            _logger?.LogError(ex, errorMessage);

            await httpContext.Response
                .WithStatusCode(StatusCodes.Status500InternalServerError)
                .WithContent(errorMessage);
        }
    }
}

public static class HttpExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseUnhandledExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UnhandledExceptionHandlerMiddleware>();
    }
}