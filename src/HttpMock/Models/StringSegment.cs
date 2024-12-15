namespace HttpMock.Models;

public readonly struct StringSegment
{
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
        if (start > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(start), "Value exceeds supported limits!");

        if (end > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(end), "Value exceed supported limits!");

        Start = (ushort)start;
        End = (ushort)end;
    }

    public ushort Start { get; init; }
    public ushort End { get; init; }

    public int Length => End - Start;
    public Range Range => Start..End;

    public static StringSegment FromRange(Range range)
    {
        if (range.Start.IsFromEnd || range.End.IsFromEnd)
            throw new ArgumentOutOfRangeException(nameof(range), "Range with IsFromEnd is not supported!");

        if (range.Start.Value > ushort.MaxValue || range.End.Value > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(range), "Range values exceed supported limits!");

        return new((ushort)range.Start.Value, (ushort)range.End.Value);
    }
}


