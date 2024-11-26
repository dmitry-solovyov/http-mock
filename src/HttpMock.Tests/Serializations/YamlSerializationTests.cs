using FluentAssertions;
using HttpMock.Models;
using HttpMock.Serializations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HttpMock.Tests.Serializations;

public class YamlSerializationTests
{
    [Fact]
    public void Serialize()
    {
        var config = new DomainConfigurationDto
        {
            Endpoints =
            [
                new EndpointConfigurationDto {
                    Method= "get", Url= "/probe", ContentType= string.Empty, Status = 201,Delay= 100 },

                new EndpointConfigurationDto {
                    Method= "get", Url="/swagger", ContentType="", Status = 201, Delay = 200 },

                new EndpointConfigurationDto {
                    Method= "post", Url="/order", ContentType="", Status = 201, Delay = 300,
                    Payload="{\"paymentId\":\"@guid\"}",
                    Headers = new Dictionary<string, string>{ { "Location", "/probe?a=b"}, { "Authorization", "Bearer aaa"} }
                }
            ]
        };

        var serializer = new YamlSerialization();

        var result = serializer.Serialize(config);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize()
    {
        var yaml = @"
Endpoints:
  - Delay: 100
    Status: 201
    Url: /probe
    
  - Url: /swagger
    Delay: 200
    Status: 201
    Foo: bar
    
  - Delay: 300
    Method: POST
    Status: 201
    Url: /order
    Payload: '{""paymentId"":""@guid""}'
    Headers:
      Location: /probe?a=b
      Authorization: 'Bearer aaa'
";

        var serializer = new YamlSerialization();

        var result = serializer.Deserialize(yaml);

        // Assert
        result.Should().NotBeNull();
        result!.Endpoints.Should().NotBeNullOrEmpty();
        result.Endpoints.Should().HaveCount(3);

        var firstItem = result.Endpoints!.ElementAt(0);
        firstItem.Delay.Should().Be(100);
        firstItem.Status.Should().Be(201);
        firstItem.Url.Should().Be("/probe");
        firstItem.Payload.Should().BeNull();
        firstItem.Headers.Should().BeNull();
        firstItem.Method.Should().BeNull();

        var secondItem = result.Endpoints!.ElementAt(1);
        secondItem.Delay.Should().Be(200);
        secondItem.Status.Should().Be(201);
        secondItem.Url.Should().Be("/swagger");
        secondItem.Payload.Should().BeNull();
        secondItem.Headers.Should().BeNull();
        secondItem.Method.Should().BeNull();

        var thirdItem = result.Endpoints!.ElementAt(2);
        thirdItem.Delay.Should().Be(300);
        thirdItem.Status.Should().Be(201);
        thirdItem.Url.Should().Be("/order");
        thirdItem.Payload.Should().Be("{\"paymentId\":\"@guid\"}");
        thirdItem.Method.Should().Be("POST");
        thirdItem.Headers.Should().NotBeNull();
        thirdItem.Headers.Should().HaveCount(2);
        thirdItem.Headers!["Location"].Should().Be("/probe?a=b");
        thirdItem.Headers!["Authorization"].Should().Be("Bearer aaa");
    }
}