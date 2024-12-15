using HttpMock.RequestProcessing;

namespace HttpMock.Models;

public readonly record struct CommandRouteDetails(CommandRequestDetails RequestDetails, ICommandRequestHandler RequestHandler);
