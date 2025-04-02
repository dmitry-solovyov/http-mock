using HttpMock.Models;
using static HttpMock.RequestHandlers.MockedRequestHandlers.MockedRequestEndpointConfigurationResolver;

namespace HttpMock.RequestHandlers.MockedRequestHandlers;

public interface IMockedRequestEndpointConfigurationResolver
{
    bool TryGetEndpointConfiguration(
        ref readonly RequestDetails requestDetails, 
        out EndpointConfiguration? foundEndpointConfiguration, out List<PathVariable>? foundVariables);
}