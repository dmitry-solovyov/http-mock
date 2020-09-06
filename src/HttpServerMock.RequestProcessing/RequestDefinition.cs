namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinition
    {
        public RequestDefinition(RequestDefinitionWhen when, RequestDefinitionThen then)
        {
            When = when;
            Then = then;
        }

        public RequestDefinitionWhen When { get; internal set; }
        public RequestDefinitionThen Then { get; internal set; }
    }
}
