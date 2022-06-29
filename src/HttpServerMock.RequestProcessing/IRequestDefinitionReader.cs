using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerMock.RequestDefinitions
{
    public interface IRequestDefinitionReader
    {
        string ContentType { get; }

        bool IsContentTypeSupported(string? contentType) => string.Equals(ContentType, contentType, System.StringComparison.OrdinalIgnoreCase);

        Task<ConfigurationDefinition> Read(Stream contentStream, CancellationToken cancellationToken = default);
    }
}