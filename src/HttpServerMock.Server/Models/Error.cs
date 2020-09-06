namespace HttpServerMock.Server.Models
{
    public sealed class Error
    {
        public Error(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
