namespace HttpMock.Models;

public readonly record struct PathRef(StringSegment Domain, StringSegment Path, StringSegment QueryParameters);