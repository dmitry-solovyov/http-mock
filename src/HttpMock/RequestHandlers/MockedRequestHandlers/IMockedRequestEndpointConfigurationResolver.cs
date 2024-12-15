using HttpMock.Models;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public interface IMockedRequestEndpointConfigurationResolver
{
    bool TryGetEndpointConfiguration(ref readonly MockedRequestDetails requestDetails, out EndpointConfiguration? foundEndpointConfiguration);
}