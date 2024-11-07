namespace HttpMock.RequestProcessing
{
    public static class HttpResponseExtensions
    {
        public static HttpResponse? WithStatusCode(this HttpResponse? httpResponse, int statusCode)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            httpResponse.StatusCode = statusCode;
            return httpResponse;
        }

        public static HttpResponse? WithContentType(this HttpResponse? httpResponse, string contentType)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            httpResponse.ContentType = contentType;
            return httpResponse;
        }

        public static HttpResponse? WithContent(this HttpResponse? httpResponse, string content, string contentType = Defaults.ContentTypes.ContentTypeForUntypedResponse)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            httpResponse.ContentType = contentType;
            if (!string.IsNullOrEmpty(content))
            {
                var data = System.Text.Encoding.UTF8.GetBytes(content);
                httpResponse.Body.Write(data);
            }
            return httpResponse;
        }

        public static async ValueTask<HttpResponse?> WithContentAsync(this HttpResponse? httpResponse,
            string? content, string contentType = Defaults.ContentTypes.ContentTypeForUntypedResponse, CancellationToken cancellationToken = default)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            httpResponse.ContentType = contentType;
            if (!string.IsNullOrEmpty(content))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                await httpResponse.Body.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            }

            return httpResponse;
        }

        public static async ValueTask<HttpResponse?> WithContentAsync(this HttpResponse? httpResponse,
            Stream contentStream, string contentType, CancellationToken cancellationToken = default)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            httpResponse.ContentType = contentType;
            await contentStream.CopyToAsync(httpResponse.Body, cancellationToken).ConfigureAwait(false);

            return httpResponse;
        }

        public static HttpResponse? WithHeaders(this HttpResponse? httpResponse, IDictionary<string, string?>? headers)
        {
            if (httpResponse?.HasStarted != false)
                return default;

            if (headers != null)
                foreach (var header in headers)
                    httpResponse.Headers.TryAdd(header.Key, header.Value);

            return httpResponse;
        }
    }
}
