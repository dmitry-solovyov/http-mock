using System;

namespace HttpServerMock.RequestDefinitions
{
    public record RequestDefinitionItem
    {
        public RequestDefinitionItem(string? description, RequestCondition when, ResponseDetails then)
        {
            Description = description;
            When = when ?? throw new ArgumentNullException(nameof(when));
            Then = then ?? throw new ArgumentNullException(nameof(then));
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string? Description { get; internal set; }
        public RequestCondition When { get; internal set; }
        public ResponseDetails Then { get; internal set; }
    }
}
