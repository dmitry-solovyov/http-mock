using HttpMock.Models;

namespace HttpMock.Extensions;

public static class ReadOnlySpanExtensions
{
    public static int IndexOfAfter(this ref readonly ReadOnlySpan<char> input, char value, int currentPosition, bool pointToEndOfRangeIfNotFound = true)
    {
        if (input.Length == 0)
            return 0;

        var nextPosition = -1;
        var offset = 0;

        if (currentPosition < input.Length)
        {
            offset = currentPosition <= 0 ? 0 : currentPosition + 1;
            var slice = input[offset..];
            nextPosition = slice.IndexOf(value);
        }

        if (nextPosition == -1)
            return pointToEndOfRangeIfNotFound ? input.Length : -1;

        return offset + nextPosition;
    }

    public static Span<Range> SplitByOld(this ref readonly ReadOnlySpan<char> input, in char separator)
    {
        if (!input.Contains(separator))
            return default;

        var occurrences = input.Count(separator);
        var partBounds = new Range[occurrences + 1];
        Span<Range> urlParameterRanges = partBounds.AsSpan();

        input.Split(partBounds, separator, StringSplitOptions.None);
        return urlParameterRanges;
    }

    public static StringSegment[] SplitBy(this ref readonly ReadOnlySpan<char> input, in char separator)
    {
        if (!input.Contains(separator))
            return [new StringSegment(0, input.Length)];

        var occurrences = input.Count(separator);
        var expectParts = occurrences + 1;
        Span<StringSegment> partBounds = stackalloc StringSegment[expectParts];

        var currentIndex = 0;
        var nextIndex = input.IndexOfAfter(separator, currentIndex);

        var occurrenceIndex = 0;
        while (currentIndex < input.Length)
        {
            if (input[currentIndex] == separator)
            {
                currentIndex++;
                nextIndex = input.IndexOfAfter(separator, currentIndex);
                continue;
            }

            if (currentIndex < nextIndex - 1)
            {
                var partRange = new StringSegment(currentIndex, nextIndex);
                partBounds[occurrenceIndex] = partRange;
                occurrenceIndex++;
            }

            currentIndex = nextIndex + 1;
            nextIndex = input.IndexOfAfter(separator, currentIndex);
        }

        if (occurrenceIndex == 0)
            return [new StringSegment(0, input.Length)];

        StringSegment[] result = new StringSegment[occurrenceIndex];
        partBounds.Slice(0, occurrenceIndex).CopyTo(result);
        return result;
    }

    public static Range[] SplitByNew(this ref readonly ReadOnlySpan<char> input, in char separator)
    {
        var occurrences = input.Count(separator);
        if (occurrences == 0)
            return [0..input.Length];

        var result = new Range[occurrences + 1];
        input.Split(result, separator, StringSplitOptions.None);
        return result;
    }
}
