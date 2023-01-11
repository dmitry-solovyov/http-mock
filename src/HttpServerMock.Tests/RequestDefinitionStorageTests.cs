using FluentAssertions;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement.Storage;
using HttpServerMock.Server.Infrastructure.RequestProcessing;
using System.Collections.Generic;
using Xunit;

namespace HttpServerMock.Tests;

public class RequestDefinitionStorageTests
{
    private readonly ConfigurationStorage _configurationStorage;

    private readonly ConfigurationStorageItemSet _configurationSet;

    private readonly HttpRequestDetails _getProbeRequestDetails = new HttpRequestDetails(
        "GET",
        "/probe", new Dictionary<string, string>(),
        "localhost",
        "application/json");

    public RequestDefinitionStorageTests()
    {
        _configurationSet = new ConfigurationStorageItemSet(
            "Test set",
            new[]
            {
                new ConfigurationStorageItem(
                    "Probe action (1)",
                    new ConfigurationStorageItemEndpointFilter(_getProbeRequestDetails.Url),
                    new ConfigurationStorageItemResponseDefinition(_getProbeRequestDetails.ContentType, _getProbeRequestDetails.HttpMethod, "{\"data\":\"response data\"}", 200, 0, null, new Dictionary<string, string>())),

                new ConfigurationStorageItem(
                    "Probe action (2)",
                    new ConfigurationStorageItemEndpointFilter(_getProbeRequestDetails.Url),
                    new ConfigurationStorageItemResponseDefinition(_getProbeRequestDetails.ContentType, _getProbeRequestDetails.HttpMethod, "{\"data\":\"response data\"}", 500, 100, null, new Dictionary<string, string>())),

                new ConfigurationStorageItem(
                    "Put data",
                    new ConfigurationStorageItemEndpointFilter("/data"),
                    new ConfigurationStorageItemResponseDefinition("application/json", "PUT", null, 201, 0, null, new Dictionary<string, string>())),
            });

        _configurationStorage = new ConfigurationStorage();
        _configurationStorage.AddSet(_configurationSet);
    }

    [Fact]
    public void FindItem_CheckExistingDefinitionItem()
    {
        // Arrange
        var requestDetails = _getProbeRequestDetails;

        var requestContext = new RequestContext(ref requestDetails);

        // Act
        var result = _configurationStorage.FindItem(ref requestContext);

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
        var result = _configurationStorage.FindItem(ref requestContext);

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
        var result = _configurationStorage.FindItem(ref requestContext);

        // Assert
        result.Should().NotBeNull();
        result!.When.Url.Should().Be(requestDetails.Url);
        result!.Then.Method.Should().Be(requestDetails.HttpMethod);
        result!.Then.ContentType.Should().Be(requestDetails.ContentType);
        result!.Then.StatusCode.Should().Be(500);
    }
}
