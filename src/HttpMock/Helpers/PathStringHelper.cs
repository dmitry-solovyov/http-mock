using HttpMock.Extensions;
using HttpMock.Models;

namespace HttpMock.Helpers;

public static class PathStringHelper
{
    private const char QuestionChar = '?';
    private const char AmpersandChar = '&';
    private const char EqualsChar = '=';
    private const char SlashChar = '/';
    private const char NumberSignChar = '#';

    public static Range GetPathWithoutQuery(ref readonly ReadOnlySpan<char> input)
    {
        var questionCharPos = input.IndexOf(QuestionChar);
        if (questionCharPos == -1)
            return 0..input.Length;

        return 0..questionCharPos;
    }

    public static PathParts GetPathParts(ref readonly ReadOnlySpan<char> input)
    {
        if (input.Length == 0)
            return default;

        var (questionCharPos, numberSignCharPos) = GetPathSplitterPositions(in input);

        var pathRange = new StringSegment(0, questionCharPos);
        var queryRange = questionCharPos == input.Length
            ? StringSegment.Empty
            : new StringSegment(questionCharPos + 1, numberSignCharPos);

        var subdirectories = GetPathSubdirectories(in input, questionCharPos);
        var parameters = GetQueryParameters(in input, questionCharPos);

        return new PathParts(
            new PathWithoutQueryPart(pathRange, subdirectories),
            new QueryParts(queryRange, parameters));
    }

    public static QueryParameterPart[]? GetQueryParameters(ref readonly ReadOnlySpan<char> input, int questionCharPos)
    {
        if (questionCharPos == -1 || questionCharPos == input.Length)
            return default;

        if (questionCharPos + 1 == input.Length)
            return default;

        var urlParamsSpan = input[(questionCharPos + 1)..input.Length];
        var separatorCount = urlParamsSpan.Count(AmpersandChar);
        var urlParametersList = new QueryParameterPart[separatorCount + 1];

        var startPosition = 0;
        var paramRangeStart = 0;

        var urlParamsRangeStart = questionCharPos + 1;
        var offset = urlParamsRangeStart;

        for (var paramIndex = 0; paramIndex <= separatorCount; paramIndex++)
        {
            urlParamsSpan = urlParamsSpan[startPosition..urlParamsSpan.Length];
            var nextPosition = urlParamsSpan.IndexOfAfter(AmpersandChar, paramRangeStart);
            Range paramRange;
            if (nextPosition == -1)
            {
                paramRange = paramRangeStart..urlParamsSpan.Length;
            }
            else
            {
                paramRange = paramRangeStart..nextPosition;
            }

            var paramSpan = urlParamsSpan[paramRange];

            var nameValueSeparatorPos = paramSpan.IndexOf(EqualsChar);
            StringSegment name, nameWithOffset, value, valueWithOffset;
            if (nameValueSeparatorPos == -1)
            {
                name = new(paramRangeStart, paramRangeStart + paramSpan.Length);

                nameWithOffset = name.WithOffset(offset);
                valueWithOffset = StringSegment.Empty;
            }
            else
            {
                name = new(paramRangeStart, nameValueSeparatorPos);
                value = new(nameValueSeparatorPos + 1, paramSpan.Length);

                nameWithOffset = name.WithOffset(offset);
                valueWithOffset = value.WithOffset(offset);
            }

            var isVariable = !valueWithOffset.IsEmpty && input[valueWithOffset.Range][0] == '@';

            urlParametersList[paramIndex] = new(
                new(nameWithOffset.Start, valueWithOffset.End), nameWithOffset, valueWithOffset, isVariable);

            if (nextPosition == -1 || (startPosition = nextPosition + 1) >= urlParamsSpan.Length)
                break;

            offset += paramRange.End.Value + 1;
        }

        return urlParametersList;
    }

    internal static (int QuestionCharPos, int NumberSignCharPos) GetPathSplitterPositions(ref readonly ReadOnlySpan<char> input)
    {
        const int startPos = 0;
        var questionCharPos = input.IndexOfAfter(QuestionChar, startPos);
        if (questionCharPos == -1)
        {
            questionCharPos = input.Length;
        }
        var numberSignCharPos = input.IndexOfAfter(NumberSignChar, questionCharPos);
        if (numberSignCharPos == -1)
        {
            numberSignCharPos = input.Length;
        }
        return (questionCharPos, numberSignCharPos);
    }

    internal static SubdirectoryPart[] GetPathSubdirectories(ref readonly ReadOnlySpan<char> input, int questionCharPos)
    {
        if (questionCharPos == 0 || input.Length == 0)
            return [];

        if (input[0] != SlashChar)
            throw new ArgumentOutOfRangeException(nameof(input), "Path should start with '/' character!");

        var querySpan = input[0..questionCharPos];
        var separatorCount = querySpan.Count(SlashChar);
        var subdirectories = new SubdirectoryPart[separatorCount];

        int startPosition = 0;

        for (var subdirectoryIndex = 0; subdirectoryIndex < separatorCount; subdirectoryIndex++)
        {
            var searchAfterIndex = startPosition == 0 ? 1 : startPosition;
            var nextPosition = querySpan.IndexOfAfter(SlashChar, searchAfterIndex);

            var subdirectory = new StringSegment(startPosition + 1, nextPosition);

            var isVariable = !subdirectory.IsEmpty &&
                subdirectory.Range.End.Value > subdirectory.Range.Start.Value + 1 &&
                input[subdirectory.Range][0] == '@';

            subdirectories[subdirectoryIndex] = new(subdirectory, isVariable);
            startPosition = nextPosition;
        }

        return subdirectories;
    }
}
