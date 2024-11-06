using FluentAssertions;
using HttpMock.Configuration;
using HttpMock.Models;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpMock.Tests.Configuration
{
    public class ConfigurationStorageTests
    {
        private const string TestDomainName = "testDomain";

        [Fact]
        public void IsDomainExists_WhenConfigurationEmpty_ExpectFalse()
        {
            var storage = new ConfigurationStorage();

            var result = storage.IsDomainExists(TestDomainName);

            result.Should().BeFalse();
        }

        private static ConfigurationStorage CreateConfigurationStorage()
        {
            var domain = CreateMockDomainConfiguration();

            var storage = new ConfigurationStorage();
            storage.ConfigureDomain(domain);
            return storage;
        }

        private static DomainConfiguration CreateMockDomainConfiguration()
        {
            EndpointConfiguration[] endpoints =
            [
                new(When: new(HttpMethodType.Get, "/api/v2"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Post, "/api/v2"),
                    Then: new(StatusCodes.Status202Accepted,"application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v3?Param1=@a&Param2=@b"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Post, "/api/v2/duplicated"),
                    Then: new(StatusCodes.Status202Accepted, "application/json")
                ),
                new(When: new(HttpMethodType.Post, "/api/v2/duplicated"),
                    Then: new(StatusCodes.Status502BadGateway, "application/json", Delay: 150)
                ),
                new(When: new(HttpMethodType.Post, "/api/v2/duplicated"),
                    Then: new(StatusCodes.Status202Accepted,"application/json")
                ),
            ];
            var domain = new DomainConfiguration(TestDomainName, endpoints);
            return domain;
        }
    }
}
