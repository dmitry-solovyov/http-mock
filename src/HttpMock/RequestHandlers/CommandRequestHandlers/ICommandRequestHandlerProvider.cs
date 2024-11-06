namespace HttpMock.RequestHandlers.CommandRequestHandlers
{
    public interface ICommandRequestHandlerProvider
    {
        Type? GetCommandHandlerType(string commandName, HttpMethodType httpMethod);
    }
}