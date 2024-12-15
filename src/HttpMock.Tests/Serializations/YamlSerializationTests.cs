using FluentAssertions;
using HttpMock.Models;
using HttpMock.Serializations;
using System.Collections.Generic;
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
                    Path= "/probe", ContentType= string.Empty, Status = 201,Delay= 100 },

                new EndpointConfigurationDto {
                    Path="/swagger", ContentType="", Status = 201, Delay = 200 },

                new EndpointConfigurationDto {
                    Path="post /order", ContentType="", Status = 201, Delay = 300,
                    Payload="{\"paymentId\":\"@guid\"}",
                    Headers = new Dictionary<string, string?>{
                        { "Location", "/probe?a=b"},
                        { "Authorization", "Bearer aaa"}
                    }
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
    Path: /probe
    
  - Path: /swagger
    Delay: 200
    Status: 201
    Foo: bar
    
  - Path: /order
    Method: POST
    Delay: 300
    Status: 201
    Payload: '{""paymentId"":""@guid""}'
    Headers:
      Location: /probe?a=b
      Authorization: 'Bearer aaa'
    
  - Path: /order?param1=@a&param2=param2Value
    Method: Post
    Status: 201
";

        var serializer = new YamlSerialization();

        var result = serializer.Deserialize(yaml);

        // Assert
        result.Should().NotBeNull();
        result!.Endpoints.Should().NotBeNullOrEmpty();
        result.Endpoints.Should().HaveCount(4);

        var firstItem = result.Endpoints![0];
        firstItem.Delay.Should().Be(100);
        firstItem.Status.Should().Be(201);
        firstItem.Path.Should().Be("/probe");
        firstItem.Payload.Should().BeNull();
        firstItem.Headers.Should().BeNull();

        var secondItem = result.Endpoints![1];
        secondItem.Delay.Should().Be(200);
        secondItem.Status.Should().Be(201);
        secondItem.Path.Should().Be("/swagger");
        secondItem.Payload.Should().BeNull();
        secondItem.Headers.Should().BeNull();

        var thirdItem = result.Endpoints![2];
        thirdItem.Delay.Should().Be(300);
        thirdItem.Status.Should().Be(201);
        thirdItem.Method.Should().Be("POST");
        thirdItem.Path.Should().Be("/order");
        thirdItem.Payload.Should().Be("{\"paymentId\":\"@guid\"}");
        thirdItem.Headers.Should().NotBeNull();
        thirdItem.Headers.Should().HaveCount(2);
        thirdItem.Headers!["Location"].Should().Be("/probe?a=b");
        thirdItem.Headers!["Authorization"].Should().Be("Bearer aaa");

        var foursItem = result.Endpoints![3];
        foursItem.Status.Should().Be(201);
        foursItem.Method.Should().Be("Post");
        foursItem.Path.Should().Be("/order?param1=@a&param2=param2Value");
    }
}
