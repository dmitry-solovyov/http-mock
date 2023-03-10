using FluentAssertions;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using HttpServerMock.Server.Infrastructure.RequestProcessing;
using HttpServerMock.Server.Infrastructure.RequestProcessing.RequestHandlers.MockedRequests;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HttpServerMock.Tests;

public class RequestHistoryStorageTests
{
    private readonly RequestHistoryStorage _requestHistoryStorage;

    private readonly Mock<IConfigurationStorage> _requestDefinitionStorageMock;

    private readonly ConfigurationStorageItem _probeGetRequestDefinitionItem;
    private readonly HttpRequestDetails _probeGetRequestDetails;

    public RequestHistoryStorageTests()
    {
        _probeGetRequestDefinitionItem = new ConfigurationStorageItem(
                    "Probe action (1)",
                    new ConfigurationStorageItemEndpointFilter("/probe"),
                    new ConfigurationStorageItemResponseDefinition("application/json", "GET", "{\"data\":\"response data\"}", 200, 0, new Dictionary<string, string>(), null, null));

        _probeGetRequestDetails = new HttpRequestDetails("GET", "/probe", new Dictionary<string, string>(), "localhost", "application/json");

        _requestDefinitionStorageMock = new Mock<IConfigurationStorage>();

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
