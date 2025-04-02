using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HttpMock.Tests.Integration;

public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        var port = 35001;
        _factory = factory.WithWebHostBuilder(builder => builder.UseUrls($"http://*:{port}/"));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Index")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        "text/html; charset=utf-8".Should().Be(response.Content.Headers.ContentType?.ToString());
    }
}