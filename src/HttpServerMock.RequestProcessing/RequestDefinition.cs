namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinition
    {
        public RequestDefinition(string description, RequestDefinitionWhen when, RequestDefinitionThen then)
        {
            Description = description;
            When = when;
            Then = then;
        }

        public string Description { get; internal set; }
        public RequestDefinitionWhen When { get; internal set; }
        public RequestDefinitionThen Then { get; internal set; }
    }
}
