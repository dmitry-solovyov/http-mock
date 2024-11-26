namespace HttpMock.RequestProcessing;

public class RequestPipelineMiddleware
{
    public RequestPipelineMiddleware(RequestDelegate next)
    {
    }

    public async Task Invoke(HttpContext? httpContext)
    {
        if (httpContext == default)
        {
            return;
        }

        var cancellationToken = httpContext.RequestAborted;

        var requestRouter = httpContext.RequestServices.GetRequiredService<IRequestRouter>();

        if (requestRouter.TryGetRouteDetails(httpContext, out var routeDetails))
        {
            await routeDetails.RequestHandler.Execute(routeDetails.RequestDetails, httpContext.Response, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            httpContext.Response.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledRequests);
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