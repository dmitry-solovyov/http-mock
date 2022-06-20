using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.Server.Infrastructure.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<string?> GetContent(this HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength == 0)
            return null;

        return await request.BodyReader.ReadPipeAsync(cancellationToken);
    }
}