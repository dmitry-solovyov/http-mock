namespace HttpMock.Serializations;

public class SerializationProvider : ISerializationProvider
{
    private readonly IEnumerable<ISerialization> _serializations;

    public SerializationProvider(IEnumerable<ISerialization> serializations)
    {
        _serializations = serializations;
    }

    public ISerialization? GetSerialization(string contentType)
    {
        return _serializations.FirstOrDefault(x => string.Equals(contentType, x.SupportedContentType, StringComparison.OrdinalIgnoreCase));
    }
}
