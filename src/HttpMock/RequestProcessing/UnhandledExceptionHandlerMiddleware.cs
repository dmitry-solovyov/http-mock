namespace HttpMock.RequestProcessing;

public class UnhandledExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

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

            httpContext.RequestServices.GetService<ILogger<UnhandledExceptionHandlerMiddleware>>()?.LogError(errorMessage);

            httpContext.Response
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