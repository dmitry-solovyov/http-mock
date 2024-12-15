using FluentAssertions;
using HttpMock.Extensions;
using System;
using Xunit;

namespace HttpMock.Tests.Extensions
{
    public class ReadOnlySpanExtensionsTests
    {
        [Theory]
        [InlineData(0, true, 3)]
        [InlineData(0, false, 3)]
        [InlineData(3, true, 7)]
        [InlineData(3, false, 7)]
        [InlineData(7, true, 11)]
        [InlineData(7, false, 11)]
        [InlineData(11, true, 13)]
        [InlineData(11, false, -1)]
        [InlineData(12, true, 13)]
        [InlineData(12, false, -1)]
        public void IndexOfAfter_SearchingFromBeginning(int searchFrom, bool endPartAsSearchResult, int expectedResult)
        {
            var s = "aaa0bbb0ccc0d".AsSpan();

            var searchFor = '0';
            var result = s.IndexOfAfter(searchFor, searchFrom, endPartAsSearchResult);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void SplitBy_EmptySpan_ExpectEmptyResult()
        {
            var p = "".AsSpan();

            var searchFor = '/';
            var result = p.SplitBy(searchFor);

            result.Length.Should().Be(1);
            result[0].Start.Should().Be(0);
            result[0].End.Should().Be(0);
        }

        [Theory]
        [InlineData("part1/part2/part3")]
        [InlineData("/part1/part2/part3")]
        [InlineData("/part1//part2///part3/")]
        [InlineData("part1//part2///part3/")]
        [InlineData("part1//part2///part3")]
        public void SplitBy_TestThreeSection(string url)
        {
            var urlSpan = url.AsSpan();

            var searchFor = '/';
            var result = urlSpan.SplitBy(searchFor);

            result.Length.Should().Be(3);
            url[result[0].Range].Should().Be("part1");
            url[result[1].Range].Should().Be("part2");
            url[result[2].Range].Should().Be("part3");
        }

        [Theory]
        [InlineData("part1")]
        [InlineData("/part1")]
        [InlineData("/part1/")]
        [InlineData("part1/")]
        public void SplitBy_TestOneSection(string url)
        {
            var urlSpan = url.AsSpan();

            var searchFor = '/';
            var result = urlSpan.SplitBy(searchFor);

            result.Length.Should().Be(1);
            url[result[0].Range].Should().Be("part1");
        }

        [Theory]
        [InlineData("", '&', 0)]
        [InlineData("part1&part2&part3", '-', 0)]
        [InlineData("part1&part2&part3", '&', 2)]
        [InlineData("part1&part2&part3", '1', 1)]
        [InlineData("&part1&part2&part3&", '&', 4)]
        public void GetOccurrencesCount(string input, char searchFor, int expectedResult)
        {
            var span = input.AsSpan();

            var result = span.GetOccurrencesCount(searchFor);

            result.Should().Be(expectedResult);
        }
    }
}
