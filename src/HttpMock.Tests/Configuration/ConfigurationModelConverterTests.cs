using FluentAssertions;
using HttpMock.Models;
using System.Collections.Generic;
using Xunit;

namespace HttpMock.Tests.Configuration
{
    public class ConfigurationModelConverterTests
    {
        [Fact]
        public void DtoToModel_ExpectDomainConfigurationCreatedWithValidSettings()
        {
            var path = "/api/v1/@urlPart?Param1=@urlValue1&Param2=urlValue2";

            var dto = CreateDomainConfigurationDto(path);

            var model = ConfigurationModelConverter.DtoToModel.Convert("test-domain", dto);

            model.Should().NotBeNull();
            model!.Domain.Should().Be("test-domain");

            model.Endpoints.Should().NotBeNullOrEmpty();
            model.Endpoints.Should().HaveCount(1);

            var endpoint = model.Endpoints[0];

            endpoint.When.Path.Should().Be(path);

            endpoint.When.QueryParameters.Should().NotBeNullOrEmpty();
            endpoint.When.QueryParameters.Should().HaveCount(2);
            var param1 = endpoint.When.QueryParameters![0];
            endpoint.When.Path[param1.Name.Range].Should().Be("Param1");
            endpoint.When.Path[param1.Value.Range].Should().Be("@urlValue1");

            endpoint.When.HttpMethod.Should().Be(HttpMethodType.Put);

            endpoint.Then.Delay.Should().Be(123);
            endpoint.Then.StatusCode.Should().Be(201);

            endpoint.Then.Headers.Should().NotBeNull();
            endpoint.Then.Headers.Should().HaveCount(2);
            endpoint.Then.Headers.Should().ContainKeys("headerKey1", "headerKey2");
        }

        private static DomainConfigurationDto CreateDomainConfigurationDto(string? path = "/api/v1/@urlPart?Param1=@urlValue1&Param2=urlValue2")
        {
            var dto = new DomainConfigurationDto
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
                        Headers = new Dictionary<string, string> {
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
