namespace HttpMock.Models;

public readonly record struct PathParts(
    PathWithoutQueryPart PathWithoutQuery,
    QueryParts Query);

public readonly record struct PathWithoutQueryPart(StringSegment Segment, SubdirectoryPart[] Subdirectories);

public readonly record struct SubdirectoryPart(StringSegment Segment, bool IsVariable);

public readonly record struct QueryParts(StringSegment Segment, QueryParameterPart[]? Parameters)
{
    public bool HasParameters => Parameters?.Length > 0;
}

public readonly record struct QueryParameterPart(StringSegment Segment, StringSegment NameSegment, StringSegment ValueSegment, bool IsVariable);
