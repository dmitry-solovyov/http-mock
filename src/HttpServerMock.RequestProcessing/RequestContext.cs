using System.Threading;

namespace HttpServerMock.RequestDefinitions
{
    public class RequestContext
    {
        public RequestContext(ref RequestDetails requestDetails, int counter = 0)
        {
            RequestDetails = requestDetails;
            _counter = counter;
        }

        public RequestDetails RequestDetails { get; }

        private int _counter;

        public int Counter
        {
            get => _counter;
            private set => IncrementCounter(value);
        }

        public RequestContext IncrementCounter(int value = 1)
        {
            Interlocked.Add(ref _counter, value);
            return this;
        }
    }
}