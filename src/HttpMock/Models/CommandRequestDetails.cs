namespace HttpMock.Models;

public readonly record struct CommandRequestDetails(string CommandName, HttpMethodType HttpMethod, string ContentType, Stream RequestBody);
