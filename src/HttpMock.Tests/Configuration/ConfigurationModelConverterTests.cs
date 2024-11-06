using FluentAssertions;
using HttpMock.Configuration;
using HttpMock.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HttpMock.Tests.Configuration
{
    public class ConfigurationModelConverterTests
    {
        [Fact]
        public void DtoToModel_ExpectDomainConfigurationCreatedWithValidSettings()
        {
            var dto = CreateDomainConfigurationDto();

            var model = ConfigurationModelConverter.DtoToModel.Convert("test-domain", dto);

            model.Should().NotBeNull();
            model!.Domain.Should().Be("test-domain");

            model.Endpoints.Should().NotBeNullOrEmpty();
            model.Endpoints.Should().HaveCount(1);

            var endpoint = model.Endpoints.ElementAt(0);
            endpoint.Description.Should().Be("Endpoint with parameters");

            endpoint.When.UrlVariables.Should().NotBeNullOrEmpty();
            endpoint.When.UrlVariables.Should().HaveCount(3);
            endpoint.When.UrlVariables.Should().Contain("urlParam1");
            endpoint.When.UrlVariables.Should().Contain("urlParam2");
            endpoint.When.UrlVariables.Should().Contain("urlParam3");

            endpoint.When.UrlRegexExpression.Should().Be("/api/v1/(?<urlParam1>[\\w]{1,}([\\w\\-\\._\\-%\\+\\*\\,\\;]){0,})\\?Param1=(?<urlParam2>[\\w]{1,}([\\w\\-\\._\\-%\\+\\*\\,\\;]){0,})&Param2=(?<urlParam3>[\\w]{1,}([\\w\\-\\._\\-%\\+\\*\\,\\;]){0,})");

            endpoint.When.HttpMethod.Should().Be(HttpMethodType.Put);

            endpoint.Then.Delay.Should().Be(123);
            endpoint.Then.StatusCode.Should().Be(201);
            endpoint.Then.CallbackUrl.Should().BeNull();
            endpoint.Then.ProxyUrl.Should().Be("http://proxy.url/");

            endpoint.Then.Headers.Should().NotBeNull();
            endpoint.Then.Headers.Should().HaveCount(2);
            endpoint.Then.Headers.Should().ContainKeys("headerKey1", "headerKey2");
        }

        private static DomainConfigurationDto CreateDomainConfigurationDto()
        {
            var dto = new DomainConfigurationDto
            {
                Endpoints =
                [
                    new EndpointConfigurationDto
                    {
                        Url = "/api/v1/@urlParam1?Param1=@urlParam2&Param2=@urlParam3",
                        Method = "put",
                        Payload = "{json}",
                        Delay = 123,
                        Status = 201,
                        ContentType = string.Empty,
                        Description = "Endpoint with parameters",
                        CallbackUrl = "test",
                        ProxyUrl = "http://proxy.url",
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
