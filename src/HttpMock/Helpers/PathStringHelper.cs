using HttpMock.Extensions;
using HttpMock.Models;

namespace HttpMock.Helpers;

public static class PathStringHelper
{
    private const char questionChar = '?';
    private const char ampersandChar = '&';
    private const char equalsChar = '=';
    private const char slashChar = '/';

    public static (StringSegment Domain, StringSegment Path) SplitDomainAndPath(ref readonly ReadOnlySpan<char> input)
    {
        if (input.Length == 0)
            return default;

        if (input[0] != slashChar)
            throw new ArgumentException("String should start with '/' character!");

        var domainEndPos = input.IndexOfAfter(slashChar, 1);
        domainEndPos = domainEndPos == -1 ? input.Length : domainEndPos;

        StringSegment domain = new(1, domainEndPos);
        StringSegment path = new(domainEndPos, input.Length);

        return (domain, path);
    }

    public static Range GetPathWithoutQuery(ref readonly ReadOnlySpan<char> input)
    {
        var questionCharPos = input.IndexOf(questionChar);
        if (questionCharPos == -1)
            return 0..input.Length;

        return 0..questionCharPos;
    }

    //TODO: parse URL fragment (/path<#...>)
    public static PathRef GetPathParts(ref readonly ReadOnlySpan<char> input, bool hasDomain)
    {
        if (input.Length == 0)
            return default;

        if (input[0] != slashChar)
            throw new ArgumentException("String should start with '/' character!");

        var questionCharPos = input.IndexOf(questionChar);
        if (questionCharPos == -1)
            questionCharPos = input.Length;

        var pathRange = GetPathRange(in input);
        var pathSpan = input[pathRange.Range];

        StringSegment domainRange;
        if (hasDomain)
        {
            var domainEndPos = pathSpan.IndexOfAfter(slashChar, 1);
            domainEndPos = domainEndPos == -1 ? questionCharPos : domainEndPos;
            domainRange = new(1, domainEndPos);
            pathRange = new(domainEndPos, questionCharPos);
        }
        else
        {
            domainRange = default;
        }

        StringSegment pathQuery;
        if (questionCharPos < input.Length)
        {
            pathQuery = new(questionCharPos + 1, input.Length);
        }
        else
        {
            pathQuery = default;
        }

        return new PathRef(domainRange, pathRange, pathQuery);
    }

    public static QueryParameterRef[]? GetQueryParametersRef(ref readonly ReadOnlySpan<char> input)
    {
        var questionCharPos = input.IndexOf(questionChar);
        if (questionCharPos == -1 || questionCharPos == input.Length)
        {
            return default;
        }

        var urlParamsRangeStart = questionCharPos + 1;

        if (questionCharPos + 1 == input.Length)
            return default;

        var urlParamsSpan = input[(questionCharPos + 1)..input.Length];
        var separatorCount = urlParamsSpan.GetOccurrencesCount(ampersandChar);
        var urlParametersList = new QueryParameterRef[separatorCount + 1];

        var startPosition = 0;
        var nextPosition = 0;
        var paramRangeStart = 0;

        var offset = urlParamsRangeStart;

        for (var paramIndex = 0; paramIndex <= separatorCount; paramIndex++)
        {
            urlParamsSpan = urlParamsSpan[startPosition..urlParamsSpan.Length];
            nextPosition = urlParamsSpan.IndexOfAfter(ampersandChar, paramRangeStart);
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

            var nameValueSeparatorPos = paramSpan.IndexOf(equalsChar);
            StringSegment name = new(paramRangeStart, nameValueSeparatorPos);
            StringSegment value = new(nameValueSeparatorPos + 1, paramSpan.Length);

            urlParametersList[paramIndex] = new(name.WithOffset(offset), value.WithOffset(offset));

            if (nextPosition == -1 || (startPosition = nextPosition + 1) >= urlParamsSpan.Length)
                break;

            offset += paramRange.End.Value + 1;
        }

        return urlParametersList;
    }

    private static StringSegment GetPathRange(ref readonly ReadOnlySpan<char> input)
    {
        const int startPos = 0;
        var questionCharPos = input.IndexOfAfter(questionChar, startPos);
        if (questionCharPos == -1 || questionCharPos == input.Length)
        {
            return new(startPos, input.Length);
        }
        return new(startPos, questionCharPos);
    }
}
