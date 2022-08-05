using FluentAssertions;
using HttpServerMock.RequestDefinitions;
using HttpServerMock.Server.Infrastructure;
using System.Collections.Generic;
using Xunit;

namespace HttpServerMock.Tests;

public class RequestDefinitionStorageTests
{
    private readonly RequestDefinitionStorage _requestDefinitionStorage;

    private readonly RequestDefinitionItemSet _defaultSet;

    private readonly RequestDetails _getProbeRequestDetails = new RequestDetails(
        "GET",
        "/probe", new Dictionary<string, string>(),
        "localhost",
        "application/json");

    public RequestDefinitionStorageTests()
    {
        _defaultSet = new RequestDefinitionItemSet(
            "Test set",
            new[]
            {
                new RequestDefinitionItem(
                    "Probe action (1)",
                    when: new RequestCondition(_getProbeRequestDetails.Url),
                    then: new ResponseDetails(_getProbeRequestDetails.ContentType, _getProbeRequestDetails.HttpMethod, "{\"data\":\"response data\"}", 200, 0, null, new Dictionary<string, string>())),

                new RequestDefinitionItem(
                    "Probe action (2)",
                    when: new RequestCondition(_getProbeRequestDetails.Url),
                    then: new ResponseDetails(_getProbeRequestDetails.ContentType, _getProbeRequestDetails.HttpMethod, "{\"data\":\"response data\"}", 500, 100, null, new Dictionary<string, string>())),

                new RequestDefinitionItem(
                    "Put data",
                    when: new RequestCondition("/data"),
                    then: new ResponseDetails("application/json", "PUT", null, 201, 0, null, new Dictionary<string, string>())),
            });

        _requestDefinitionStorage = new RequestDefinitionStorage();
        _requestDefinitionStorage.AddSet(_defaultSet);
    }

    [Fact]
    public void FindItem_CheckExistingDefinitionItem()
    {
        // Arrange
        var requestDetails = _getProbeRequestDetails;

        var requestContext = new RequestContext(ref requestDetails);

        // Act
        var result = _requestDefinitionStorage.FindItem(ref requestContext);

        // Assert
        result.Should().NotBeNull();
        result!.When.Url.Should().Be(requestDetails.Url);
        result!.Then.Method.Should().Be(requestDetails.HttpMethod);
        result!.Then.ContentType.Should().Be(requestDetails.ContentType);
        result!.Then.StatusCode.Should().Be(200);
    }

    [Fact]
    public void FindItem_CheckExistingDefinitionItem_WithPreDefinedIndex()
    {
        // Arrange
        var requestDetails = _getProbeRequestDetails;

        var requestContext = new RequestContext(ref requestDetails, 2);

        // Act
        var result = _requestDefinitionStorage.FindItem(ref requestContext);

        // Assert
        result.Should().NotBeNull();
        result!.When.Url.Should().Be(requestDetails.Url);
        result!.Then.Method.Should().Be(requestDetails.HttpMethod);
        result!.Then.ContentType.Should().Be(requestDetails.ContentType);
        result!.Then.StatusCode.Should().Be(500);
    }

    [Fact]
    public void FindItem_CheckExistingDefinitionItem_WithPreDefinedIndexMoreThenFoundItems()
    {
        // Arrange
        var requestDetails = _getProbeRequestDetails;

        var requestContext = new RequestContext(ref requestDetails, 12);

        // Act
        var result = _requestDefinitionStorage.FindItem(ref requestContext);

        // Assert
        result.Should().NotBeNull();
        result!.When.Url.Should().Be(requestDetails.Url);
        result!.Then.Method.Should().Be(requestDetails.HttpMethod);
        result!.Then.ContentType.Should().Be(requestDetails.ContentType);
        result!.Then.StatusCode.Should().Be(500);
    }
}
