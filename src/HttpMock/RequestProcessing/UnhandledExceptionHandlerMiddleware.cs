using System.Net.Mime;

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
        catch(Exception ex)
        {
            httpContext.RequestServices.GetService<ILogger<UnhandledExceptionHandlerMiddleware>>()?
                .LogError($"Unhandled exception {ex.Message}{Environment.NewLine}{ex.StackTrace}!");

            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if (!httpContext.Response.HasStarted)
            {
                await httpContext.Response.WriteAsync("An unexpected error occurred!", CancellationToken.None);
            }
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