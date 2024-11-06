namespace HttpMock.Serializations
{
    public interface ISerializationProvider
    {
        ISerialization? GetSerialization(string contentType);
    }
}