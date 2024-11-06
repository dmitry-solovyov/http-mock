namespace HttpMock.RequestProcessing;

public class RequestPipelineMiddleware
{
    private readonly RequestDelegate _next;

    public RequestPipelineMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext? httpContext)
    {
        if (httpContext == default)
            return;

        var cancellationToken = httpContext.RequestAborted;

        var requestRouter = httpContext.RequestServices.GetRequiredService<IRequestRouter>();

        if (requestRouter.TryGetRouteDetails(httpContext, out var routeDetails))
        {
            await routeDetails.RequestHandler.Execute(routeDetails.RequestDetails, httpContext.Request, httpContext.Response, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await httpContext.Response.FillContentAsync(StatusCodes.Status501NotImplemented, cancellationToken).ConfigureAwait(false);
        }

        await httpContext.Response.CompleteAsync().ConfigureAwait(false);
    }
}

public static class RequestHandlerPipelineMiddlewareExtensions
{
    public static void UseRequestPipeline(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestPipelineMiddleware>();
    }
}