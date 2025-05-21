using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.RegularExpressions;

namespace HttpMock.Tests.Integration;

public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        var port = 35001;
        _factory = factory.WithWebHostBuilder(builder => builder.UseSetting("port", port.ToString()));

        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/index")]
    public async Task Configuration_TestEmptyGet(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        //response.EnsureSuccessStatusCode(); // Status Code 200-299
        //"text/html; charset=utf-8".Should().Be(response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task TestConfiguration_SimpleSetup()
    {
        // Arrange
        var putResponse = await PutConfiguration(TestConfiguration);
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        // Act
        var response = await GetConfiguration();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        "application/yaml".Should().Be(response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task TestConfiguration_GetProbeEndpoint()
    {
        // Arrange
        var putResponse = await PutConfiguration(TestConfiguration);
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        // Act
        var response = await GetMockedEndpoint(new Uri("/probe", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        "application/json".Should().Be(response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task TestConfiguration_GetEndpointWithTwoSetups()
    {
        // Arrange
        var putResponse = await PutConfiguration(TestConfiguration);
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        var endpointAddress = new Uri("/test?id=123", UriKind.Relative);

        // Act and Assert
        var response1 = await GetMockedEndpoint(endpointAddress);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var responseContent1 = await response1.Content.ReadAsStringAsync();
        var regex = new Regex("\\{\"testId\":\"123\",\"uuid\":\"([a-zA-Z0-9-]+)\"\\}");
        regex.IsMatch(responseContent1).Should().BeTrue();

        var response2 = await GetMockedEndpoint(endpointAddress);
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        var response3 = await GetMockedEndpoint(endpointAddress);
        response3.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("?param1=1&param2=2&param3=3", "{\"p1\":\"1\",\"p2\":\"2\",\"p3\":\"3\",\"path1\":\"path1\",\"path2\":\"path2\",\"path3\":\"@path3\"}")]
    [InlineData("?param3=3&param2=2&param1=1", "{\"p1\":\"1\",\"p2\":\"2\",\"p3\":\"3\",\"path1\":\"path1\",\"path2\":\"path2\",\"path3\":\"@path3\"}")]
    [InlineData("?param2=2&param3=3&param1=1", "{\"p1\":\"1\",\"p2\":\"2\",\"p3\":\"3\",\"path1\":\"path1\",\"path2\":\"path2\",\"path3\":\"@path3\"}")]
    [InlineData("?param3=3&param1=1&param2=2", "{\"p1\":\"1\",\"p2\":\"2\",\"p3\":\"3\",\"path1\":\"path1\",\"path2\":\"path2\",\"path3\":\"@path3\"}")]
    public async Task ConfigurationWithDifferentParametersOrder_TestGet(string path, string expectedContent)
    {
        // Arrange
        var putResponse = await PutConfiguration(ConfigurationWithDifferentParametersOrder);
        putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Accepted);

        var endpointAddress = new Uri($"/api/path1/path2/endpoint{path}", UriKind.Relative);

        // Act and Assert
        var response = await GetMockedEndpoint(endpointAddress);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        expectedContent.Should().Be(responseContent);
    }

    private async Task<HttpResponseMessage> PutConfiguration(string content)
    {
        var putRequestMessage = new HttpRequestMessage(HttpMethod.Put, "/");
        putRequestMessage.Headers.Add(@"x-httpmock-command", "configurations");
        putRequestMessage.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/yaml");

        return await _client.SendAsync(putRequestMessage);
    }

    private async Task<HttpResponseMessage> GetConfiguration()
    {
        var getRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/");
        getRequestMessage.Headers.Add(@"x-httpmock-command", "configurations");
        getRequestMessage.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/yaml");

        return await _client.SendAsync(getRequestMessage);
    }

    private async Task<HttpResponseMessage> GetMockedEndpoint(Uri requestUrl)
    {
        var getRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        return await _client.SendAsync(getRequestMessage);
    }

    private const string TestConfiguration =
        """
        Endpoints:
        - Path: /test?id=@id
          Method: get
          Status: 201
          Description: successful probe action
          Payload: '{"testId":"@id","uuid":"@guid"}'

        - Path: /test?id=@id
          Method: get
          Status: 202
          Description: successful probe action
          Payload: '{"testId":"@id"}'

        - Path: /probe
          Method: get
          Status: 200
          Description: successful probe action
        """;

    private const string ConfigurationWithDifferentParametersOrder =
        """
        Endpoints:
        - Path: /api/@path1/@path2/endpoint?param1=@p1&param2=@p2&param3=@p3
          Method: get
          Status: 200
          Description: successful probe action
          Payload: '{"p1":"@p1","p2":"@p2","p3":"@p3","path1":"@path1","path2":"@path2","path3":"@path3"}'
        """;
}