﻿namespace HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers;

public record struct RequestHandlerContext(HttpRequestDetails RequestDetails, IRequestHandler RequestHandler);
