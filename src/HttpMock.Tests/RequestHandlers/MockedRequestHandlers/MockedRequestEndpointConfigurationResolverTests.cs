using FluentAssertions;
using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestHandlers.MockedRequestHandlers;
using HttpMock.RequestProcessing;
using Microsoft.AspNetCore.Http;
using System.IO;
using Xunit;

namespace HttpMock.Tests.RequestHandlers.MockedRequestHandlers
{
    public class MockedRequestEndpointConfigurationResolverTests
    {
        private const string TestDomainName = "testDomain";

        [Fact]
        public void TryGetEndpointConfiguration_ExpectStatisticsUpdated()
        {
            var resolver = CreateMockedRequestEndpointConfigurationResolver();

            var requestDetails = new RequestDetails(string.Empty, TestDomainName, HttpMethodType.Post, "/api/v2", "application/json", new MemoryStream());

            var result = resolver.TryGetEndpointConfiguration(ref requestDetails, out var foundEndpointConfiguration);
            result.Should().BeTrue();

            foundEndpointConfiguration.Should().NotBeNull();
            foundEndpointConfiguration!.CallCounter.Should().Be(1);

            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);

            foundEndpointConfiguration.Should().NotBeNull();
            foundEndpointConfiguration!.CallCounter.Should().Be(5);
        }

        [Fact]
        public void TryGetEndpointConfiguration_WhenDuplicatedDefinitionExists_ExpectEndpointsSelectedOneByOne()
        {
            var resolver = CreateMockedRequestEndpointConfigurationResolver();

            var requestDetails = new RequestDetails(string.Empty, TestDomainName, HttpMethodType.Post, "/api/v2/duplicated", "application/json", new MemoryStream());

            // first iteration through the duplicated endpoints
            var result = resolver.TryGetEndpointConfiguration(ref requestDetails, out var foundEndpointConfiguration);
            result.Should().BeTrue();

            foundEndpointConfiguration.Should().NotBeNull();
            foundEndpointConfiguration!.CallCounter.Should().Be(1);
            foundEndpointConfiguration.Then.Delay.Should().Be(0);

            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            foundEndpointConfiguration!.CallCounter.Should().Be(1);
            foundEndpointConfiguration.Then.Delay.Should().Be(150);

            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            foundEndpointConfiguration!.CallCounter.Should().Be(1);
            foundEndpointConfiguration.Then.Delay.Should().Be(0);

            // second iteration through the duplicated endpoints
            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            foundEndpointConfiguration!.CallCounter.Should().Be(2);
            foundEndpointConfiguration.Then.Delay.Should().Be(0);

            _ = resolver.TryGetEndpointConfiguration(ref requestDetails, out foundEndpointConfiguration);
            foundEndpointConfiguration!.CallCounter.Should().Be(2);
            foundEndpointConfiguration.Then.Delay.Should().Be(150);
        }

        private static MockedRequestEndpointConfigurationResolver CreateMockedRequestEndpointConfigurationResolver(ConfigurationStorage? configurationStorage = default)
        {
            var storage = configurationStorage ?? CreateConfigurationStorage();
            var resolver = new MockedRequestEndpointConfigurationResolver(storage);
            return resolver;
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
