using HttpServerMock.RequestDefinitions;

namespace HttpServerMock.Server.Models
{
    public class RequestDetails : IRequestDetails
    {
        private static readonly Lazy<IReadOnlyDictionary<string, string>> EmptyDictionaryGetter =
            new Lazy<IReadOnlyDictionary<string, string>>(() => new Dictionary<string, string>(capacity: 0));

        public RequestDetails(string httpMethod, string uri, IReadOnlyDictionary<string, string> headers, string? clientAddress, string? contentType)
        {
            HttpMethod = httpMethod;
            Uri = uri;
            Headers = headers ?? EmptyDictionaryGetter.Value;
            ClientAddress = clientAddress;
            ContentType = contentType;
        }

        public string HttpMethod { get; }
        public string Uri { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public string? ContentType { get; }
        public string? ClientAddress { get; }
    }
}