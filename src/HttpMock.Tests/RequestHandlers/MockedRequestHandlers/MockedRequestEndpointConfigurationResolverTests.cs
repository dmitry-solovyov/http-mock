using FluentAssertions;
using HttpMock.Configuration;
using HttpMock.Models;
using HttpMock.RequestHandlers.MockedRequestHandlers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpMock.Tests.RequestHandlers.MockedRequestHandlers
{
    public class MockedRequestEndpointConfigurationResolverTests
    {
        private const string TestDomainName = "testDomain";

        [Fact]
        public void TryGetEndpointConfiguration_EmptyQueryParameters_ExpectStatisticsUpdated()
        {
            var resolver = CreateMockedRequestEndpointConfigurationResolver();

            var requestDetails = new MockedRequestDetails(TestDomainName, HttpMethodType.Get, "/api/v1");

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

            var requestDetails = new MockedRequestDetails(TestDomainName, HttpMethodType.Post, "/api/v7/duplicated");

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

        [Theory]
        [InlineData("/api/v1", true)]
        [InlineData("/api/v1?param1=value1", true)]
        [InlineData("/api/v2?param1=value1", true)]
        [InlineData("/api/v3?param1=value1", false)]
        [InlineData("/api/v3?param2=value3", false)]
        [InlineData("/api/v3?param2=value2", true)]
        [InlineData("/api/v4?param2=value2&param1=value1&PARAM3=VALUE3", true)]
        [InlineData("/api/v4?param1=value1&param2=value2&PARAM3=VALUE3", true)]
        [InlineData("/api/v4?param1=&param2=value2&PARAM3=VALUE3", true)]
        [InlineData("/api/v4?param1=value1&param2=value2", false)]
        [InlineData("/api/v5/aaa/items?param1=value1&param2=value2", true)]
        [InlineData("/api/v5/bbb/items?param2=value2&param1=value1", true)]
        [InlineData("/api/v5/bbb/items?param1=value1", true)]
        [InlineData("/api/v5/bbb/items", true)]
        [InlineData("/api/v6/aaa/items", true)]
        [InlineData("/api/v6/aaa/items?param1=value", true)]
        public void TryGetEndpointConfiguration_ExpectEndpointsSelectedForSpecifiedQuery(string queryParameter, bool expectedResult)
        {
            var resolver = CreateMockedRequestEndpointConfigurationResolver();

            var requestDetails = new MockedRequestDetails(TestDomainName, HttpMethodType.Get, queryParameter);

            // first iteration through the duplicated endpoints
            var result = resolver.TryGetEndpointConfiguration(ref requestDetails, out var foundEndpointConfiguration);
            result.Should().Be(expectedResult);
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
                new(When: new(HttpMethodType.Get, "/api/v1"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v2?Param1=@a&Param2=@b"),
                    Then: new(StatusCodes.Status202Accepted,"application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v3?Param2=value2&Param1=@a"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v4?Param3=value3&Param2=value2&Param1=@a"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v5/@code/items?Param1=@a&Param2=@b"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Get, "/api/v6/@code/items"),
                    Then: new(StatusCodes.Status200OK, "application/json")
                ),
                new(When: new(HttpMethodType.Post, "/api/v7/duplicated"),
                    Then: new(StatusCodes.Status202Accepted, "application/json")
                ),
                new(When: new(HttpMethodType.Post, "/api/v7/@duplicated" ),
                    Then: new(StatusCodes.Status502BadGateway, "application/json", Delay: 150)
                ),
                new(When: new(HttpMethodType.Post, "/api/v7/duplicated"),
                    Then: new(StatusCodes.Status202Accepted,"application/json")
                ),
            ];

            var domain = new DomainConfiguration(TestDomainName, endpoints);
            return domain;
        }
    }
}
