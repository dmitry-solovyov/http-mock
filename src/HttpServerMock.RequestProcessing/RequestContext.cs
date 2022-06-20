using System;
using System.Threading;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestContext
    {
        public RequestContext(IRequestDetails requestDetails, int counter = 0)
        {
            RequestDetails = requestDetails ?? throw new ArgumentNullException(nameof(requestDetails));
            Counter = counter;
            RequestDefinition = null;
        }

        public RequestContext(RequestContext mockedRequest, RequestDefinitionItem requestDefinition)
        {
            RequestDetails = mockedRequest.RequestDetails ?? throw new ArgumentNullException(nameof(mockedRequest));
            Counter = mockedRequest.Counter;
            RequestDefinition = requestDefinition ?? throw new ArgumentNullException(nameof(RequestDefinition));
        }

        public IRequestDetails RequestDetails { get; }
        public RequestDefinitionItem? RequestDefinition { get; }

        private int _counter;

        public int Counter
        {
            get => _counter;
            private set => Interlocked.Exchange(ref _counter, value);
        }

        public RequestContext IncrementCounter()
        {
            Interlocked.Add(ref _counter, 1);
            return this;
        }
    }
}