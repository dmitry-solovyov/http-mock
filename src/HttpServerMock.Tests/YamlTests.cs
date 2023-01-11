using FluentAssertions;
using HttpServerMock.Server.Infrastructure.ConfigurationManagement;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HttpServerMock.Tests;

public class YamlTests
{
    [Fact]
    public void Serialize()
    {
        var config = new ConfigurationBatchDto();
        config.Info = "Test data";
        config.Map = new List<ConfigurationItemDto>
        {
            new ConfigurationItemDto
            {
                Url = "/probe",
                Status = 200,
                Delay = 100,
                Description = "Probe endpoint"
            },
            new ConfigurationItemDto
            {
                Url = "/swagger",
                Status = 200,
                Delay = 200,
                Description= "Swagger endpoint"
            },
            new ConfigurationItemDto
            {
                Url = "/order",
                Status = 201,
                Delay = 300,
                Description = "Order endpoint",
                Payload = "{\"paymentId\":\"@guid\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Location", "/probe?a=b"},
                    { "Authorization", "Bearer aaa"}
                }
            }
        };

        var serializer = new SharpYaml.Serialization.Serializer(new SharpYaml.Serialization.SerializerSettings
        {
            EmitAlias = false,
            EmitShortTypeName = false,
            DefaultStyle = SharpYaml.YamlStyle.Block,
        });

        var result = serializer.Serialize(config);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize()
    {
        var yaml = @"
Info: Test data
Map:
  - Delay: 100
    Description: Probe endpoint
    Status: 200
    Url: /probe
    
  - Delay: 200
    Description: Swagger endpoint
    Status: 200
    Url: /swagger
    Foo: bar
    
  - Delay: 300
    Method: POST
    Description: Order endpoint
    Status: 201
    Url: /order
    Payload: '{""paymentId"":""@guid""}'
    Headers:
      Location: /probe?a=b
      Authorization: 'Bearer aaa'
";

        var serializer = new SharpYaml.Serialization.Serializer();
        serializer.Settings.IgnoreUnmatchedProperties = true;

        var result = serializer.Deserialize<ConfigurationBatchDto>(yaml);

        // Assert
        result.Should().NotBeNull();
        result.Info.Should().Be("Test data");
        result.Map.Should().NotBeNullOrEmpty();
        result.Map.Should().HaveCount(3);

        var firstItem = result.Map!.ElementAt(0);
        firstItem.Delay.Should().Be(100);
        firstItem.Description.Should().Be("Probe endpoint");
        firstItem.Status.Should().Be(200);
        firstItem.Url.Should().Be("/probe");
        firstItem.Payload.Should().BeNull();
        firstItem.Headers.Should().BeNull();
        firstItem.Method.Should().BeNull();

        var secondItem = result.Map!.ElementAt(1);
        secondItem.Delay.Should().Be(200);
        secondItem.Description.Should().Be("Swagger endpoint");
        secondItem.Status.Should().Be(200);
        secondItem.Url.Should().Be("/swagger");
        secondItem.Payload.Should().BeNull();
        secondItem.Headers.Should().BeNull();
        secondItem.Method.Should().BeNull();

        var thirdItem = result.Map!.ElementAt(2);
        thirdItem.Delay.Should().Be(300);
        thirdItem.Description.Should().Be("Order endpoint");
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