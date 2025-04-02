using HttpMock.Models;

namespace HttpMock.Extensions;

public static class RangeExtensions
{
    public static StringSegment WithOffset(this StringSegment segment, int offset)
    {
        return new(segment.Start + offset, segment.End + offset);
    }
}
