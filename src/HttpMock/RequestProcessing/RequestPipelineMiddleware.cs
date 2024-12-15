namespace HttpMock.RequestProcessing;

public class RequestPipelineMiddleware
{
    public RequestPipelineMiddleware(RequestDelegate _)
    {
    }

#pragma warning disable S2325
    public async Task Invoke(HttpContext? httpContext)
    {
        if (httpContext == default)
            return;

        var cancellationToken = httpContext.RequestAborted;

        var router = httpContext.RequestServices.GetRequiredService<IRequestRouter>();

        if (router.TryGetCommandRouteDetails(httpContext, out var commandRouteDetails))
        {
            await commandRouteDetails.RequestHandler.Execute(commandRouteDetails.RequestDetails, httpContext.Response, cancellationToken).ConfigureAwait(false);
        }
        else if (router.TryGetMockedRouteDetails(httpContext, out var mockedRouteDetails))
        {
            await mockedRouteDetails.RequestHandler.Execute(mockedRouteDetails.RequestDetails, httpContext.Response, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            httpContext.Response.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledRequests);
        }

        await httpContext.Response.CompleteAsync().ConfigureAwait(false);
    }
}
#pragma warning restore S2325

public static class RequestHandlerPipelineMiddlewareExtensions
{
    public static void UseRequestPipeline(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestPipelineMiddleware>();
    }
}