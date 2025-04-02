using HttpMock.RequestProcessing;

namespace HttpMock.Models;

public readonly record struct RouteDetails(RequestDetails RequestDetails, IMockedRequestHandler RequestHandler);
