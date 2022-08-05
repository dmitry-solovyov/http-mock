using FluentAssertions;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HttpServerMock.Tests;

public class RequestHistoryStorageTests
{
    private readonly RequestHistoryStorage _requestHistoryStorage;

    private readonly Mock<IRequestDefinitionStorage> _requestDefinitionStorageMock;

    private readonly RequestDefinitionItem _probeGetRequestDefinitionItem;
    private readonly RequestDetails _probeGetRequestDetails;

    public RequestHistoryStorageTests()
    {
        _probeGetRequestDefinitionItem = new RequestDefinitionItem(
                    "Probe action (1)",
                    when: new RequestCondition("/probe"),
                    then: new ResponseDetails("application/json", "GET", "{\"data\":\"response data\"}", 200, 0, null, new Dictionary<string, string>()));

        _probeGetRequestDetails = new RequestDetails("GET", "/probe", new Dictionary<string, string>(), "localhost", "application/json");

        _requestDefinitionStorageMock = new Mock<IRequestDefinitionStorage>();

        _requestHistoryStorage = new RequestHistoryStorage(_requestDefinitionStorageMock.Object);
    }

    [Fact]
    public void GetMockedRequestWithDefinition()
    {
        var requestDetails = _probeGetRequestDetails;

        _requestDefinitionStorageMock
            .Setup(call => call.FindItem(ref It.Ref<RequestContext>.IsAny))
            .Returns(_probeGetRequestDefinitionItem);

        // Act
        var result = _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);

        // Assert
        result.Should().NotBeNull();
        result.RequestDefinition.Should().Be(_probeGetRequestDefinitionItem);
    }

    [Fact]
    public void GetMockedRequestWithDefinition_MultipleCalls()
    {
        var requestDetails = _probeGetRequestDetails;

        _requestDefinitionStorageMock
            .Setup(call => call.FindItem(ref It.Ref<RequestContext>.IsAny))
            .Returns(_probeGetRequestDefinitionItem);

        // Act
        _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);
        _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);
        _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);
        var result = _requestHistoryStorage.GetMockedRequestWithDefinition(ref requestDetails);

        // Assert
        result.Should().NotBeNull();
        result.RequestDefinition.Should().Be(_probeGetRequestDefinitionItem);
        result.MockedRequest.Counter.Should().Be(4);
    }
}
