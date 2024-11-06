using System.Net.Mime;

namespace HttpMock.RequestProcessing
{
    public static class HttpResponseExtensions
    {
        private const string DefaultContentType = MediaTypeNames.Text.Plain;

        public static void FillContent(this HttpResponse httpResponse,
            int statusCode, string? content = default, string contentType = DefaultContentType, CancellationToken cancellationToken = default)
        {
            if (httpResponse.HasStarted)
                return;

            httpResponse.StatusCode = statusCode;
            httpResponse.ContentType = contentType;

            if (!string.IsNullOrEmpty(content))
            {
                var data = System.Text.Encoding.UTF8.GetBytes(content);
                httpResponse.Body.Write(data);
            }
        }

        public static ValueTask FillContentAsync(this HttpResponse httpResponse, 
            int statusCode, CancellationToken cancellationToken = default)
        {
            return FillContentAsync(httpResponse, statusCode, default(string), DefaultContentType, cancellationToken);
        }

        public static ValueTask FillContentAsync(this HttpResponse httpResponse,
            int statusCode, string? content, CancellationToken cancellationToken = default)
        {
            return FillContentAsync(httpResponse, statusCode, content, DefaultContentType, cancellationToken);
        }

        public static async ValueTask FillContentAsync(this HttpResponse httpResponse,
            int statusCode, string? content = default, string contentType = DefaultContentType, CancellationToken cancellationToken = default)
        {
            if (httpResponse.HasStarted)
                return;

            httpResponse.StatusCode = statusCode;
            httpResponse.ContentType = contentType;

            if (!string.IsNullOrEmpty(content))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                await httpResponse.Body.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
        }

        public static async ValueTask FillContentAsync(this HttpResponse httpResponse,
            int statusCode, Stream contentStream, string contentType = DefaultContentType, CancellationToken cancellationToken = default)
        {
            if (httpResponse.HasStarted)
                return;

            httpResponse.StatusCode = statusCode;
            httpResponse.ContentType = contentType;

            await contentStream.CopyToAsync(httpResponse.Body, cancellationToken).ConfigureAwait(false);
        }

        public static void FillHeaders(this HttpResponse httpResponse, IDictionary<string, string>? headers = default)
        {
            if (!httpResponse.HasStarted && headers?.Any() == true)
            {
                foreach (var header in headers)
                    httpResponse.Headers.TryAdd(header.Key, header.Value);
            }
        }
    }
}
