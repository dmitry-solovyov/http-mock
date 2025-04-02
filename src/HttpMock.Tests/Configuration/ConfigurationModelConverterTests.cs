using FluentAssertions;
using HttpMock.Models;
using System.Collections.Generic;
using Xunit;

namespace HttpMock.Tests.Configuration
{
    public class ConfigurationModelConverterTests
    {
        [Theory]
        [InlineData("/api/v1/@urlPart?Param1=@urlValue1&Param2=urlValue2")]
        public void DtoToModel_ExpectConfigurationCreatedWithValidSettings(string originalPath)
        {
            var dto = CreateConfigurationDto(originalPath);

            // Act
            var model = ConfigurationModelConverter.DtoToModel.Convert(dto);

            // Arrange
            model.Should().NotBeNull();

            model!.Endpoints.Should().NotBeNullOrEmpty();
            model.Endpoints.Should().HaveCount(1);

            var endpoint = model.Endpoints[0];

            endpoint.When.Path.Should().Be(originalPath);
            endpoint.When.PathParts.Should().NotBeNull();

            endpoint.When.HttpMethod.Should().Be(HttpMethodType.Put);

            endpoint.Then.Delay.Should().Be(123);
            endpoint.Then.StatusCode.Should().Be(201);

            endpoint.Then.Headers.Should().NotBeNull();
            endpoint.Then.Headers.Should().HaveCount(2);
            endpoint.Then.Headers.Should().ContainKeys("headerKey1", "headerKey2");
        }

        private static ConfigurationDto CreateConfigurationDto(string? path = "/api/v1/@urlPart?Param1=@urlValue1&Param2=urlValue2")
        {
            var dto = new ConfigurationDto
            {
                Endpoints =
                [
                    new EndpointConfigurationDto
                    {
                        Path = path,
                        Method = "put",
                        Payload = "{json}",
                        Delay = 123,
                        Status = 201,
                        ContentType = string.Empty,
                        Headers = new Dictionary<string, string?> {
                            { "headerKey1", "value1" },
                            { "headerKey2", "value2" },
                        }
                    }
                ]
            };

            return dto;
        }
    }
}
