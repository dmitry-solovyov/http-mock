using HttpMock.Models;
using HttpMock.RequestProcessing;

namespace HttpMock.RequestHandlers.MockedRequestHandlers
{
    public interface IMockedRequestEndpointConfigurationResolver
    {
        bool TryGetEndpointConfiguration(ref readonly RequestDetails requestDetails, out EndpointConfiguration? endpointConfiguration);
    }
}