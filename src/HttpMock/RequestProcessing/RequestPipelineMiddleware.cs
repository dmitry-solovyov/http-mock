namespace HttpMock.RequestProcessing;

public class RequestPipelineMiddleware
{
    public RequestPipelineMiddleware(RequestDelegate _)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "<Pending>")]
    public async Task Invoke(HttpContext? httpContext)
    {
        if (httpContext == default)
            return;

        var cancellationToken = httpContext.RequestAborted;

        var router = httpContext.RequestServices.GetRequiredService<IRequestRouter>();

        var isHandled = await router.TryExecuteRequestHandler(httpContext, cancellationToken).ConfigureAwait(false);
        if(!isHandled)
        {
            httpContext.Response.WithStatusCode(Defaults.StatusCodes.StatusCodeForUnhandledRequests);
        }

        await httpContext.Response.CompleteAsync().ConfigureAwait(false);
    }
}
