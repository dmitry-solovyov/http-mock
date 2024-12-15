using HttpMock.RequestProcessing;

namespace HttpMock.Models;

public readonly record struct MockedRouteDetails(MockedRequestDetails RequestDetails, IMockedRequestHandler RequestHandler);
