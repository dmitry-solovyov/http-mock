namespace HttpMock.Models;

public readonly record struct CommandRequestDetails(
    string CommandName, string Domain, HttpMethodType HttpMethod, string ContentType, Stream RequestBody);
