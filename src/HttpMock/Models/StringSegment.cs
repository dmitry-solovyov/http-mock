namespace HttpMock.Models;

public readonly struct StringSegment
{
    private static Lazy<StringSegment> EmptySegment = new(() => new(0, 0));

    public StringSegment()
    {
        Start = 0;
        End = 0;
    }

    public StringSegment(ushort start, ushort end)
    {
        Start = start;
        End = end;
    }

    public StringSegment(int start, int end)
    {
        if (start < 0 || start > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(start), "Start value exceeds supported limits!");

        if (end < 0 || end > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(end), "End value exceed supported limits!");

        Start = (ushort)start;
        End = (ushort)end;
    }

    public StringSegment(Range range)
    {
        if (range.Start.IsFromEnd || range.End.IsFromEnd)
            throw new ArgumentOutOfRangeException(nameof(range), "Range with IsFromEnd is not supported!");

        if (range.Start.Value > ushort.MaxValue || range.End.Value > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(range), "Range values exceed supported limits!");

        Start = (ushort)range.Start.Value;
        End = (ushort)range.End.Value;
    }

    public ushort Start { get; init; }
    public ushort End { get; init; }

    public int Length => End - Start;
    public Range Range => Start..End;

    public bool IsEmpty => End == Start;

    public static StringSegment Empty => EmptySegment.Value;
}


