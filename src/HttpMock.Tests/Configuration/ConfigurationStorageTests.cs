using FluentAssertions;
using HttpMock.Configuration;
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
    }
}
