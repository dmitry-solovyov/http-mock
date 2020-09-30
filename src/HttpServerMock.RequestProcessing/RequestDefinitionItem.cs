using System;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestDefinitionItem
    {
        public RequestDefinitionItem(string description, RequestDefinitionWhen when, RequestDefinitionThen then)
        {
            Description = description;
            When = when;
            Then = then;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Description { get; internal set; }
        public RequestDefinitionWhen When { get; internal set; }
        public RequestDefinitionThen Then { get; internal set; }
    }
}
