using FluentAssertions;
using HttpMock.Models;
using Xunit;

namespace HttpMock.Tests.Models;

public class RequestDetailsTests
{
    [Fact]
    public void PathParts_SameInstanceInCopiedInstance()
    {
        var requestDetails = new RequestDetails(HttpMethodType.Get, "/a/b/c/d");

        var pathParts = requestDetails.PathParts;

        var copyOfRequestDetails = requestDetails with { };

        var pathPartsFromCopy = copyOfRequestDetails.PathParts;

        pathPartsFromCopy.Should().Be(pathParts);
    }
}
